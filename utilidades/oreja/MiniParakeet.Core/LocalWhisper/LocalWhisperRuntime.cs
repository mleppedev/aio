using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using MiniParakeet.Core.Transcription;

namespace MiniParakeet.Core.LocalWhisper;

/// <summary>
/// Runtime wrapper que mantiene un contexto Whisper y permite decodificación incremental.
/// </summary>
internal sealed class LocalWhisperRuntime : IAsyncDisposable
{
    private readonly GgmlType _modelType;
    private readonly string _modelDir;
    private WhisperProcessor? _processor;
    private WhisperFactory? _factory;
    private readonly ConcurrentQueue<float[]> _pending = new();
    private Task? _loopTask;
    private CancellationTokenSource? _cts;
    // Intervalo base entre decodificaciones (latencia objetivo ~0.4-0.6s)
    private readonly TimeSpan _decodeInterval = TimeSpan.FromMilliseconds(400);
    // Decodificación anticipada si acumulamos ~0.4s de audio (16kHz mono)
    private const int _earlySamples = 6400;
    private const double _earlyRmsThreshold = 0.0025;
    private float[] _accumulator = Array.Empty<float>();
    private readonly object _accSync = new();
    private string _lastPartial = string.Empty;
    private DateTime _lastSpeech = DateTime.MinValue;
    // Aumentamos umbral para evitar cortar frases en pausas cortas y permitir frases más largas.
    private readonly TimeSpan _silenceFinalize = TimeSpan.FromMilliseconds(1000); // si silencio > 1s, finalizamos
    private const int MaxWindowSamples = 16000 * 8; // mantener hasta 8s de contexto para no truncar frases largas

    public event EventHandler<TranscriptionUpdateEventArgs>? TranscriptionUpdated;

    public LocalWhisperRuntime(string modelNameOrSize, string? modelDirectory = null)
    {
        _modelType = modelNameOrSize.ToLower() switch
        {
            "tiny" => GgmlType.Tiny,
            "base" => GgmlType.Base,
            "small" => GgmlType.Small,
            "medium" => GgmlType.Medium,
            "large" or "large-v2" => GgmlType.LargeV2,
            _ => GgmlType.Tiny
        };
        _modelDir = modelDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniParakeet", "models");
    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_modelDir);
        var modelPath = Path.Combine(_modelDir, $"ggml-{_modelType.ToString().ToLower()}.bin");
        if (!File.Exists(modelPath))
        {
            await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(_modelType, cancellationToken: ct);
            await using var fs = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fs, ct);
        }
        // Diagnóstico: verificar presencia de whisper.dll nativo (lo despliega el paquete en la carpeta de ejecución/bin).
    // Mensaje simple de inicialización (evitamos confundir con nombres de dll específicos).
    TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs("[diag] Whisper local inicializado", true, TimeSpan.Zero));
        try
        {
            _factory = WhisperFactory.FromPath(modelPath);
            _processor = _factory.CreateBuilder()
                .WithLanguage("auto")
                .Build();
        }
        catch (DllNotFoundException ex)
        {
            TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs("[error] Faltan binarios nativos Whisper: " + ex.Message + " (asegura paquete Whisper.net.runtime.win-x64)", true, TimeSpan.Zero));
            throw;
        }
        catch (Exception ex)
        {
            TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs("[error] Init Whisper falló: " + ex.Message, true, TimeSpan.Zero));
            throw;
        }
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loopTask = Task.Run(() => DecodeLoop(_cts.Token));
    }

    public void Push(float[] pcmMono16k)
    {
        _pending.Enqueue(pcmMono16k);
    }

    private async Task DecodeLoop(CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (!ct.IsCancellationRequested)
        {
            while (_pending.TryDequeue(out var chunk))
            {
                lock (_accSync)
                {
                    if (_accumulator.Length == 0)
                        _accumulator = chunk;
                    else
                    {
                        var merged = new float[_accumulator.Length + chunk.Length];
                        Buffer.BlockCopy(_accumulator, 0, merged, 0, _accumulator.Length * sizeof(float));
                        Buffer.BlockCopy(chunk, 0, merged, _accumulator.Length * sizeof(float), chunk.Length * sizeof(float));
                        _accumulator = merged;
                    }
                }
            }

        bool timeReady;
        int accLen;
        lock (_accSync) accLen = _accumulator.Length;
        timeReady = sw.Elapsed >= _decodeInterval;
            bool lengthReady = accLen >= _earlySamples;
            if (_processor != null && (timeReady || lengthReady))
            {
                float[] snapshot;
                bool empty;
                lock (_accSync)
                {
                    empty = _accumulator.Length == 0;
                    snapshot = _accumulator;
                }
                if (empty)
                {
                    sw.Restart();
                    await Task.Delay(50, ct);
                    continue;
                }
                try
                {
                    // Filtro de silencio: calcular RMS rápido
                    double sumSq = 0;
                    for (int i = 0; i < snapshot.Length; i += 160) // submuestreo para velocidad
                    {
                        var v = snapshot[i];
                        sumSq += v * v;
                    }
                    var rms = Math.Sqrt(sumSq / Math.Max(1, snapshot.Length / 160));
                    bool isSilence = rms < (lengthReady ? 0.0012 : 0.0018);
                    string assembled = string.Empty;
                    if (!isSilence)
                    {
                        await foreach (var res in _processor.ProcessAsync(snapshot, ct))
                        {
                            assembled += res.Text;
                        }
                        if (!string.IsNullOrWhiteSpace(assembled))
                        {
                            _lastSpeech = DateTime.UtcNow;
                            var trimmed = assembled.Trim();
                            if (!trimmed.StartsWith(_lastPartial))
                            {
                                // reinicio si whisper reemite distinto
                                _lastPartial = trimmed;
                            }
                            else
                            {
                                _lastPartial = trimmed; // extendido
                            }
                            // Emitir parcial (IsFinal=false)
                            TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs(_lastPartial, false, sw.Elapsed));
                        }
                    }
                    else
                    {
                        // Silencio: si tenemos texto parcial y ya pasó el timeout -> final
                        if (!string.IsNullOrWhiteSpace(_lastPartial) && (DateTime.UtcNow - _lastSpeech) > _silenceFinalize)
                        {
                            TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs(_lastPartial, true, sw.Elapsed));
                            _lastPartial = string.Empty;
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    TranscriptionUpdated?.Invoke(this, new TranscriptionUpdateEventArgs($"[whisper error] {ex.Message}", true, sw.Elapsed));
                }
                finally
                {
                    lock (_accSync)
                    {
                        // Si emitimos final, limpiamos; si no, conservamos ventana (ya procesada) para continuidad.
                        if (string.IsNullOrEmpty(_lastPartial))
                        {
                            _accumulator = Array.Empty<float>();
                        }
                        else
                        {
                            if (_accumulator.Length > MaxWindowSamples)
                            {
                                var trimmed = new float[MaxWindowSamples];
                                Buffer.BlockCopy(_accumulator, (_accumulator.Length - MaxWindowSamples) * sizeof(float), trimmed, 0, MaxWindowSamples * sizeof(float));
                                _accumulator = trimmed;
                            }
                        }
                    }
                    sw.Restart();
                }
            }
            await Task.Delay(50, ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { _cts?.Cancel(); } catch { }
        if (_loopTask != null) { try { await _loopTask; } catch { } }
        _processor?.Dispose();
        _factory?.Dispose();
    }
}
