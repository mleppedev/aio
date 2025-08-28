using System;
using System.Threading;
using System.Threading.Channels;

namespace MiniParakeet.Core.Audio;

public sealed class AudioChunker
{
    private readonly int _chunkMilliseconds;
    private readonly int _sampleRate;
    private readonly int _channels;
    private readonly int _bytesPerSample;

    public AudioChunker(int chunkMilliseconds = 320, int sampleRate = 48000, int channels = 2, int bytesPerSample = 2)
    {
        _chunkMilliseconds = chunkMilliseconds;
        _sampleRate = sampleRate;
        _channels = channels;
        _bytesPerSample = bytesPerSample;
    }

    public async Task RunAsync(ChannelReader<byte[]> input, ChannelWriter<byte[]> output, CancellationToken ct = default)
    {
        var bytesPerMs = _sampleRate * _channels * _bytesPerSample / 1000;
        var targetBytes = bytesPerMs * _chunkMilliseconds;
        var buffer = new MemoryStream();
        try
        {
            await foreach (var segment in input.ReadAllAsync(ct))
            {
                buffer.Write(segment, 0, segment.Length);
                if (buffer.Length >= targetBytes)
                {
                    var data = buffer.ToArray();
                    await output.WriteAsync(data, ct);
                    buffer.SetLength(0);
                }
            }

            if (buffer.Length > 0)
            {
                var data = buffer.ToArray();
                await output.WriteAsync(data, ct);
            }
        }
        finally
        {
            output.TryComplete();
            buffer.Dispose();
        }
    }
}
