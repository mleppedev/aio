using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MiniParakeet.Core.Transcription;

namespace MiniParakeet.Core.LocalWhisper;

/// <summary>
/// Proveedor local real usando Whisper.net (modelo GGML) con resample a 16k mono y decodificación periódica.
/// </summary>
public sealed class LocalWhisperProvider : ITranscriptionProvider
{
    private readonly LocalWhisperRuntime _runtime;
    private readonly int _srcSampleRate;
    private readonly int _srcChannels;
    private bool _started;

    public event EventHandler<TranscriptionUpdateEventArgs>? TranscriptionUpdated;

    public LocalWhisperProvider(string modelSize = "tiny", int sourceSampleRate = 48000, int sourceChannels = 2)
    {
        _runtime = new LocalWhisperRuntime(modelSize);
        _runtime.TranscriptionUpdated += (s, e) => TranscriptionUpdated?.Invoke(this, e);
        _srcSampleRate = sourceSampleRate;
        _srcChannels = sourceChannels;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_started) return;
        _started = true;
        await _runtime.InitializeAsync(ct);
    }

    public Task PushChunkAsync(byte[] pcmChunk, CancellationToken ct = default)
    {
        if (!_started || pcmChunk.Length == 0) return Task.CompletedTask;
        // Convertir bytes PCM16 interleaved -> float[] mono 16k
    // Convertir a short[]
    int samples = pcmChunk.Length / 2;
    var shorts = new short[samples];
    Buffer.BlockCopy(pcmChunk, 0, shorts, 0, pcmChunk.Length);
    var floats = PcmResampler.ToMono16k(shorts, _srcSampleRate, _srcChannels);
        if (floats.Length > 0)
            _runtime.Push(floats);
        return Task.CompletedTask;
    }

    public Task CompleteAsync(CancellationToken ct = default) => Task.CompletedTask;

    public async ValueTask DisposeAsync() => await _runtime.DisposeAsync();
}
