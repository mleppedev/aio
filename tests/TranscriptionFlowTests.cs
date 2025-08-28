using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MiniParakeet.Core.Transcription;
using Xunit;

public class TranscriptionFlowTests
{
    [Fact]
    public async Task LocalWhisper_Accepts_Audio_And_Fires_Final()
    {
        var provider = new MiniParakeet.Core.LocalWhisper.LocalWhisperProvider("tiny", 48000, 2);
        var updates = new List<TranscriptionUpdateEventArgs>();
        provider.TranscriptionUpdated += (_, e) => updates.Add(e);
        await provider.StartAsync();
        // generar 0.5s de silencio 48kHz stereo 16bit -> 48000*2*2*0.5 = 96000 bytes
        await provider.PushChunkAsync(new byte[96000]);
        await Task.Delay(1200); // dar tiempo al bucle interno para decodificar
        Assert.Contains(updates, u => u.IsFinal || !string.IsNullOrWhiteSpace(u.Text));
    }
}
