using System.Text;

namespace MiniParakeet.Core.Chat;

public static class PromptBuilder
{
    public static string BuildSummaryPrompt(string context, int maxWords, string instructionTemplate)
    {
        if (maxWords <= 0) maxWords = 60;
        var tmpl = string.IsNullOrWhiteSpace(instructionTemplate)
            ? "eres candidato a un cargo como senior software developer, responde la pregunta (máx {max} palabras)." : instructionTemplate;
        tmpl = tmpl.Replace("{max}", maxWords.ToString());
        var sb = new StringBuilder();
        sb.AppendLine(tmpl);
        sb.AppendLine("---");
        sb.AppendLine("CONTEXT:");
        sb.AppendLine(context);
        sb.AppendLine("---");
        sb.AppendLine("Fin del contexto.");
        return sb.ToString();
    }
}
