using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Transcription;

/// <summary>
/// Implementación simple que acumula audio y envía a la API de OpenAI Whisper en ventanas crecientes.
/// Nota: La API oficial es batch; para efecto semi-streaming hacemos llamadas periódicas.
/// </summary>
public sealed class OpenAIWhisperProvider : ITranscriptionProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly List<byte> _buffer = new();
    private readonly object _sync = new();
    // Parámetros de segmentación por silencio
    private const double _silenceRmsThreshold = 0.008; // ajustable (PCM16 normalizado)
    private static readonly TimeSpan _silenceHold = TimeSpan.FromMilliseconds(650); // silencio continuo para cortar
    private static readonly TimeSpan _maxSegment = TimeSpan.FromSeconds(15); // forzar envío
    private static readonly TimeSpan _loopTick = TimeSpan.FromMilliseconds(250);
    private const double _overlapSeconds = 0.28; // retener 280ms de contexto post-envío
    private readonly int _sampleRate;
    private readonly int _channels;
    private readonly int _bitsPerSample;
    private DateTime _lastSpeech = DateTime.UtcNow;
    private DateTime _segmentStart = DateTime.UtcNow;
    private static readonly TimeSpan _minFirstSegment = TimeSpan.FromSeconds(3); // no enviar antes de 3s salvo flush manual
    private bool _completed;
    private Task? _loopTask;
    private CancellationTokenSource? _cts;
    private DateTime _lastFinalEmit = DateTime.MinValue;
    private string _lastFinalText = string.Empty;
    private static readonly HashSet<string> _noiseSingles = new(StringComparer.OrdinalIgnoreCase){"you","um","uh","ah","eh"};

    public event EventHandler<TranscriptionUpdateEventArgs>? TranscriptionUpdated;

    public OpenAIWhisperProvider(HttpClient http, string apiKey, int sampleRate = 48000, int channels = 2, int bitsPerSample = 16)
    {
        _http = http;
        _apiKey = apiKey;
        _sampleRate = sampleRate;
        _channels = channels;
        _bitsPerSample = bitsPerSample;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _segmentStart = DateTime.UtcNow;
        _lastSpeech = DateTime.UtcNow;
        _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    public Task PushChunkAsync(byte[] pcmChunk, CancellationToken ct = default)
    {
        if (pcmChunk.Length == 0 || _completed) return Task.CompletedTask;
        // Calcular RMS rápido del chunk para detectar voz
        double sumSq = 0;
        int samples = 0;
        for (int i = 0; i + 1 < pcmChunk.Length; i += 2 * _channels) // tomar solo canal izquierdo si stereo
        {
            short sample = (short)(pcmChunk[i] | (pcmChunk[i + 1] << 8));
            double f = sample / 32768.0;
            sumSq += f * f;
            samples++;
        }
        double rms = samples > 0 ? Math.Sqrt(sumSq / samples) : 0;
        if (rms > _silenceRmsThreshold)
            _lastSpeech = DateTime.UtcNow;
        lock (_sync) { _buffer.AddRange(pcmChunk); }
        return Task.CompletedTask;
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !_completed)
        {
            await Task.Delay(_loopTick, ct);
            TimeSpan sinceSpeech = DateTime.UtcNow - _lastSpeech;
            TimeSpan segmentDur = DateTime.UtcNow - _segmentStart;
            bool shouldFlush;
            byte[] snapshot;
            lock (_sync)
            {
                var enoughAudio = segmentDur >= _minFirstSegment; // asegurar 3s de contexto inicial
                shouldFlush = enoughAudio && (_buffer.Count > _sampleRate * _channels * (_bitsPerSample/8) * 0.6) &&
                              (sinceSpeech >= _silenceHold || segmentDur >= _maxSegment);
                if (!shouldFlush) continue;
                snapshot = _buffer.ToArray();
            }
            try
            {
                var text = await TranscribeAsync(snapshot, ct);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var trimmed = text.Trim();
                    // Heurística: descartar falsos positivos de una sola palabra de ruido repetidos rápidamente
                    if (_noiseSingles.Contains(trimmed) && (DateTime.UtcNow - _lastFinalEmit) < TimeSpan.FromSeconds(3))
                    {
                        // ignorar
                    }
                    else if (_noiseSingles.Contains(trimmed) && snapshot.Length < (_sampleRate * _channels * (_bitsPerSample/8)) )
                    {
                        // segmento muy corto (<1s) y palabra ruido -> ignorar
                    }
                    else
                    {
                        // Eliminar repeticiones finales del mismo token sobre la última línea
                        if (trimmed.Equals(_lastFinalText, StringComparison.OrdinalIgnoreCase) && (DateTime.UtcNow - _lastFinalEmit) < TimeSpan.FromSeconds(2))
                        {
                            // duplicado inmediato, saltar
                        }
                        else
                        {
                            TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs(trimmed, true, segmentDur));
                            _lastFinalEmit = DateTime.UtcNow;
                            _lastFinalText = trimmed;
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs($"[api error] {ex.Message}", true, segmentDur));
            }
            finally
            {
                // Retener solapamiento para continuidad de palabra
                var bytesPerSecond = _sampleRate * _channels * (_bitsPerSample / 8);
                int keep = (int)(bytesPerSecond * _overlapSeconds);
                lock (_sync)
                {
                    if (_buffer.Count <= keep) { }
                    else
                    {
                        var start = _buffer.Count - keep;
                        _buffer.RemoveRange(0, start);
                    }
                }
                _segmentStart = DateTime.UtcNow;
            }
        }
    }

    private async Task<string> TranscribeAsync(byte[] pcm, CancellationToken ct)
    {
        // Convertir a WAV PCM16 mono (si está stereo se mantiene) - API requiere formato file multipart.
        using var msWav = new MemoryStream();
        WriteWavHeader(msWav, (short)_channels, _sampleRate, (short)_bitsPerSample, pcm.Length);
        await msWav.WriteAsync(pcm, ct);
        msWav.Position = 0;
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(msWav);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(streamContent, "file", "audio.wav");
        content.Add(new StringContent("whisper-1"), "model");
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        req.Content = content;
        using var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("text", out var textEl))
            {
                var txt = textEl.GetString() ?? string.Empty;
                return txt.Replace("\n", " ").Trim();
            }
        }
        catch { }
        return string.Empty;
    }

    private static void WriteWavHeader(Stream stream, short channels, int sampleRate, short bitsPerSample, int dataLength)
    {
        var blockAlign = (short)(channels * (bitsPerSample / 8));
        var byteRate = sampleRate * blockAlign;
        using var bw = new BinaryWriter(stream, System.Text.Encoding.ASCII, true);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + dataLength);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
        bw.Write(16); // PCM chunk size
        bw.Write((short)1); // PCM
        bw.Write(channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write(bitsPerSample);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(dataLength);
    }

    public Task CompleteAsync(CancellationToken ct = default)
    {
        _completed = true;
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    // Flush manual solicitado por UI
    public async Task ForceFlushAsync(CancellationToken ct = default)
    {
        byte[] snapshot;
        lock (_sync)
        {
            if (_buffer.Count == 0) return;
            snapshot = _buffer.ToArray();
            _buffer.Clear();
        }
        try
        {
            var text = await TranscribeAsync(snapshot, ct);
            if (!string.IsNullOrWhiteSpace(text))
            {
                TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs(text.Trim(), true, DateTime.UtcNow - _segmentStart));
            }
        }
        catch (Exception ex)
        {
            TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs("[api error] " + ex.Message, true, TimeSpan.Zero));
        }
        finally { _segmentStart = DateTime.UtcNow; }
    }

    public async ValueTask DisposeAsync()
    {
        try { _completed = true; _cts?.Cancel(); } catch { }
        if (_loopTask != null) { try { await _loopTask; } catch { } }
    }
}
