using MiniParakeet.Core.Config;
using Xunit;
using System.IO;
using System.Threading.Tasks;

public class SettingsStoreTests
{
    [Fact]
    public async Task Save_And_Load_Preserves_ApiKey()
    {
        var temp = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore(temp);
            store.SetApiKeyPlain("TESTKEY");
            await store.SaveAsync();
            var store2 = new SettingsStore(temp);
            await store2.LoadAsync();
            var key = store2.GetApiKeyPlain();
            Assert.Equal("TESTKEY", key);
        }
        finally { File.Delete(temp); }
    }
}
