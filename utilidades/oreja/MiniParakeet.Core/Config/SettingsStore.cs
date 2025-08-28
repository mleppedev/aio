using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace MiniParakeet.Core.Config;

public class AppSettings
{
    public string? EncryptedApiKey { get; set; }
    public string PromptTemplate { get; set; } = "eres candidato a un cargo como senior software developer, responde la pregunta (máx {max} palabras)";
    public int MaxWords { get; set; } = 60;
    public int ContextWindowSeconds { get; set; } = 120;
    public bool EphemeralMode { get; set; } = false;
    // Default ahora: false = usar OpenAI al arrancar (si hay API key); fallback a local si no hay key.
    public bool UseLocalWhisper { get; set; } = false;
    public string LocalWhisperModel { get; set; } = "tiny"; // tiny, base, small, medium, large
}

public sealed class SettingsStore
{
    private readonly string _filePath;
    public AppSettings Current { get; private set; } = new();

    public SettingsStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniParakeet", "settings.json");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath)) return;
        var json = await File.ReadAllTextAsync(_filePath);
        var loaded = JsonSerializer.Deserialize<AppSettings>(json);
        if (loaded != null) Current = loaded;
    }

    public async Task SaveAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public void SetApiKeyPlain(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) { Current.EncryptedApiKey = null; return; }
        var raw = Encoding.UTF8.GetBytes(apiKey);
        var enc = Dpapi.Protect(raw);
        Current.EncryptedApiKey = Convert.ToBase64String(enc);
    }

    public string? GetApiKeyPlain()
    {
        if (string.IsNullOrWhiteSpace(Current.EncryptedApiKey)) return null;
        try
        {
            var data = Dpapi.Unprotect(Convert.FromBase64String(Current.EncryptedApiKey));
            return Encoding.UTF8.GetString(data);
        }
        catch { return null; }
    }

    private static class Dpapi
    {
        [StructLayout(LayoutKind.Sequential)] private struct DATA_BLOB { public int cbData; public IntPtr pbData; }
        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)] private static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string? szDataDescr, IntPtr pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, ref DATA_BLOB pDataOut);
        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)] private static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, string? szDataDescr, IntPtr pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, int dwFlags, ref DATA_BLOB pDataOut);
        [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr hMem);

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;

        public static byte[] Protect(byte[] data) => Transform(data, true);
        public static byte[] Unprotect(byte[] data) => Transform(data, false);

        private static byte[] Transform(byte[] input, bool protect)
        {
            var inBlob = new DATA_BLOB();
            var outBlob = new DATA_BLOB();
            try
            {
                inBlob.cbData = input.Length;
                inBlob.pbData = Marshal.AllocHGlobal(input.Length);
                Marshal.Copy(input, 0, inBlob.pbData, input.Length);
                bool ok = protect
                    ? CryptProtectData(ref inBlob, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, ref outBlob)
                    : CryptUnprotectData(ref inBlob, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, ref outBlob);
                if (!ok || outBlob.pbData == IntPtr.Zero) return input; // fallback
                var result = new byte[outBlob.cbData];
                Marshal.Copy(outBlob.pbData, result, 0, outBlob.cbData);
                return result;
            }
            finally
            {
                if (inBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(inBlob.pbData);
                if (outBlob.pbData != IntPtr.Zero) LocalFree(outBlob.pbData);
            }
        }
    }
}
