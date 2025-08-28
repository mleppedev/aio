using System;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;

namespace MiniParakeet.Core.Audio;

using NAudio.CoreAudioApi;
using NAudio.Wave;

/// <summary>
/// Captura loopback WASAPI real (audio que suena en los altavoces) y entrega PCM 16-bit little-endian.
/// Emite bloques ~100ms para pipeline streaming.
/// </summary>
public sealed class WasapiLoopbackCapturer : IAudioCapturer
{
    private WasapiLoopbackCapture? _capture;
    private volatile bool _paused;
    private ChannelWriter<byte[]>? _writer;
    private CancellationTokenSource? _cts;
    private bool _convertFloat32ToPcm16;

    public int SampleRate { get; private set; } = 48000;
    public int Channels { get; private set; } = 2;
    public int BitsPerSample { get; } = 16;

    public event EventHandler<double>? LevelAvailable; // RMS 0..1

    public void Start(ChannelWriter<byte[]> channel, CancellationToken ct = default)
    {
        if (_capture != null) return;
        _writer = channel;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _capture = new WasapiLoopbackCapture();
        SampleRate = _capture.WaveFormat.SampleRate;
        Channels = _capture.WaveFormat.Channels;
    // Muchos dispositivos exponen float32; convertimos a PCM16 para el pipeline.
    _convertFloat32ToPcm16 = _capture.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat && _capture.WaveFormat.BitsPerSample == 32;
        _capture.DataAvailable += OnData;
        _capture.RecordingStopped += (s, e) => channel.TryComplete(e?.Exception);
        _capture.StartRecording();
    }

    private void OnData(object? sender, WaveInEventArgs e)
    {
        if (_paused || e.BytesRecorded <= 0 || _writer == null) return;
        byte[] arr;
        if (_convertFloat32ToPcm16)
        {
            int floatCount = e.BytesRecorded / 4;
            var shorts = new short[floatCount];
            // Convertir sample a sample (evitar BitConverter por overhead en bucle grande usando unsafe opcional)
            for (int i = 0; i < floatCount; i++)
            {
                float f = BitConverter.ToSingle(e.Buffer, i * 4);
                if (f > 1f) f = 1f; else if (f < -1f) f = -1f;
                shorts[i] = (short)(f * 32767f);
            }
            arr = new byte[shorts.Length * 2];
            Buffer.BlockCopy(shorts, 0, arr, 0, arr.Length);
            // Calcular RMS rápido (submuestreo)
            double sumSq = 0; int stride = Math.Max(1, shorts.Length / 400);
            for (int i = 0; i < shorts.Length; i += stride)
            {
                var v = shorts[i] / 32768.0; sumSq += v * v;
            }
            double rms = Math.Sqrt(sumSq / (shorts.Length / (double)stride + 1));
            LevelAvailable?.Invoke(this, rms);
        }
        else
        {
            arr = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, arr, 0, e.BytesRecorded);
            // Calcular RMS para PCM16 directamente
            int samples = e.BytesRecorded / 2;
            double sumSq = 0; int stride = Math.Max(1, samples / 400);
            for (int i = 0; i < samples; i += stride)
            {
                short s16 = BitConverter.ToInt16(e.Buffer, i * 2);
                var v = s16 / 32768.0; sumSq += v * v;
            }
            double rms = Math.Sqrt(sumSq / (samples / (double)stride + 1));
            LevelAvailable?.Invoke(this, rms);
        }
        // Escritura no bloqueante: si está lleno, intentar drop parcial para baja latencia
        if (!_writer.TryWrite(arr))
        {
            // Intento fallback asincrónico
            _ = Task.Run(async () =>
            {
                try { await _writer!.WriteAsync(arr, _cts!.Token); } catch { }
            });
        }
    }

    public void Pause() => _paused = true;
    public void Resume() => _paused = false;

    public async ValueTask DisposeAsync()
    {
        try { _paused = true; } catch { }
        try { _cts?.Cancel(); } catch { }
        if (_capture != null)
        {
            try { _capture.StopRecording(); } catch { }
            _capture.DataAvailable -= OnData;
            _capture.Dispose();
            _capture = null;
        }
        // complete channel if not already
        _writer?.TryComplete();
        await Task.CompletedTask;
    }
}
