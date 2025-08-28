using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using MiniParakeet.Core.Audio;
using Xunit;

public class AudioChunkerTests
{
    [Fact]
    public async Task Splits_By_Configured_Size()
    {
        var input = Channel.CreateUnbounded<byte[]>();
        var output = Channel.CreateUnbounded<byte[]>();
        var chunker = new AudioChunker(chunkMilliseconds: 100, sampleRate: 1000, channels: 1, bytesPerSample: 1);
        var cts = new CancellationTokenSource();

        var runTask = chunker.RunAsync(input.Reader, output.Writer, cts.Token);

        // bytesPerMs = 1000*1*1/1000 =1, targetBytes=100
        await input.Writer.WriteAsync(new byte[30]);
        await input.Writer.WriteAsync(new byte[30]);
        await input.Writer.WriteAsync(new byte[40]); // total 100 -> flush
        await input.Writer.WriteAsync(new byte[50]); // partial
        input.Writer.Complete();

        var chunks = new List<byte[]>();
        await foreach (var c in output.Reader.ReadAllAsync())
            chunks.Add(c);

        Assert.Equal(2, chunks.Count); // 100 + 50 final
        Assert.Equal(100, chunks[0].Length);
        Assert.Equal(50, chunks[1].Length);
    }
}
