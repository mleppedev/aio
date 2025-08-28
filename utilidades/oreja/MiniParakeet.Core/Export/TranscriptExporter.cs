using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Export;

public static class TranscriptExporter
{
    public static async Task<string> ExportAsync(string transcript, string? directory = null)
    {
        directory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MiniParakeet");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, $"transcript_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt");
        await File.WriteAllTextAsync(file, transcript, Encoding.UTF8);
        return file;
    }
}
