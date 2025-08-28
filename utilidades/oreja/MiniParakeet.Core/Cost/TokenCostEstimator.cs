using System;

namespace MiniParakeet.Core.Cost;

public sealed class TokenCostEstimator
{
    // Heurística: ~4 chars por token inglés promedio
    private readonly double _charsPerToken;
    private readonly decimal _pricePer1KTokens;

    public TokenCostEstimator(double charsPerToken = 4.0, decimal pricePer1KTokens = 0.006m)
    {
        _charsPerToken = charsPerToken;
        _pricePer1KTokens = pricePer1KTokens;
    }

    public (int tokens, decimal cost) Estimate(string text)
    {
        if (string.IsNullOrEmpty(text)) return (0, 0m);
        int tokens = (int)Math.Ceiling(text.Length / _charsPerToken);
        var cost = (_pricePer1KTokens / 1000m) * tokens;
        return (tokens, decimal.Round(cost, 6));
    }
}
