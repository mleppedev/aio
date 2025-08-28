using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Chat;

/// <summary>
/// Resumen offline heurístico: extrae oraciones clave por frecuencia de términos y longitud.
/// </summary>
public sealed class OfflineSummaryProvider : IChatProvider
{
    public Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        // Buscar segmento CONTEXT:
        var idx = prompt.IndexOf("CONTEXT:", StringComparison.OrdinalIgnoreCase);
        var ctx = idx >= 0 ? prompt[(idx + 8)..] : prompt;
        // Normalizar
    var sentences = Regex.Split(ctx, "(?<=[.!?])\\s+").Where(s => s.Trim().Length > 0).Take(50).ToList();
        if (sentences.Count == 0) return Task.FromResult("(sin contenido)");
        var wordFreq = sentences.SelectMany(s => Regex.Matches(s.ToLowerInvariant(), "[a-záéíóúüñ0-9]+").Select(m => m.Value))
            .GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
        double Score(string s)
        {
            var words = Regex.Matches(s.ToLowerInvariant(), "[a-záéíóúüñ0-9]+").Select(m => m.Value);
            double freq = words.Sum(w => wordFreq.TryGetValue(w, out var c) ? c : 0);
            return freq / Math.Max(1, s.Length) * 1000 + Math.Min(120, s.Length) * 0.01; // mezcla frecuencia + densidad
        }
        var top = sentences.OrderByDescending(Score).Take(3);
        var summary = string.Join(" ", top).Trim();
        return Task.FromResult(summary);
    }
}
