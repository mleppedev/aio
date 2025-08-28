using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Updates;

public sealed class UpdateChecker
{
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public UpdateChecker(HttpClient http, string endpoint)
    {
        _http = http; _endpoint = endpoint;
    }

    public async Task<string?> GetLatestVersionAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(_endpoint);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("version", out var v)) return v.GetString();
        }
        catch { }
        return null;
    }
}
