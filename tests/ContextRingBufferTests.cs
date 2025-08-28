using MiniParakeet.Core.Context;
using System.Threading.Tasks;
using Xunit;

public class ContextRingBufferTests
{
    [Fact]
    public async Task Removes_Old_Entries_Beyond_Capacity()
    {
        var buf = new ContextRingBuffer(1); // 1 segundo
        buf.Add("uno");
        Assert.Equal(1, buf.Count);
        await Task.Delay(1100);
        buf.Add("dos");
        var win = buf.GetWindow(2);
        Assert.DoesNotContain("uno", win);
        Assert.Contains("dos", win);
    }
}
