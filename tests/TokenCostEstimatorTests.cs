using MiniParakeet.Core.Cost;
using Xunit;

public class TokenCostEstimatorTests
{
    [Fact]
    public void Estimates_Tokens_And_Cost()
    {
        var est = new TokenCostEstimator(charsPerToken:4, pricePer1KTokens:0.01m);
        var (tokens, cost) = est.Estimate("abcdefghij"); // 10 chars -> 3 tokens (ceil 2.5)
        Assert.True(tokens >= 2);
        Assert.True(cost > 0m);
    }
}
