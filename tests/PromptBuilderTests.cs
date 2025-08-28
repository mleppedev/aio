using MiniParakeet.Core.Chat;
using Xunit;

public class PromptBuilderTests
{
    [Fact]
    public void Replaces_Max_Placeholder()
    {
        var prompt = PromptBuilder.BuildSummaryPrompt("Hola mundo", 25, "Resume en {max} palabras");
        Assert.Contains("25", prompt);
        Assert.Contains("CONTEXT:", prompt);
    }
}
