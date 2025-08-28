using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Chat;

/// <summary>
/// Implementación básica usando OpenAI Chat Completions API.
/// </summary>
public sealed class OpenAIChatProvider : IChatProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAIChatProvider(HttpClient http, string apiKey, string model = "gpt-4o-mini")
    {
        _http = http;
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        var body = new
        {
            model = _model,
            messages = new object[]
            {
                new { role = "system", content = "Eres un asistente que resume brevemente el contexto dado."},
                new { role = "user", content = prompt }
            },
            temperature = 0.2,
            max_tokens = 180
        };
        var json = JsonSerializer.Serialize(body);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.SendAsync(req, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            // Intentar extraer mensaje de error estándar { error: { message, type, code }}
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    var em = err.TryGetProperty("message", out var mEl) ? mEl.GetString() : raw;
                    var code = (int)resp.StatusCode;
                    return $"[api error {code}] {em}";
                }
            }
            catch { }
            return $"[api error {(int)resp.StatusCode}] {resp.StatusCode}: {raw}";
        }
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var contentEl))
                {
                    var txt = contentEl.GetString() ?? string.Empty;
                    return txt.Replace("\n", " ").Trim();
                }
            }
        }
        catch (Exception ex)
        {
            return "(parse error) " + ex.Message;
        }
        return "(sin respuesta)";
    }
}
