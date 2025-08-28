using System;
using System.Windows;
using MiniParakeet.App.UI;

namespace MiniParakeet.App;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
    LoadDotEnv();
    var app = new System.Windows.Application();
        MainWindow? mainWindow = null;
        TrayApp? tray = null;
        GlobalHotkeyRegistrar? registrar = null;
        app.Startup += (_, _) =>
        {
            mainWindow = new MainWindow();
            mainWindow.Show();
            tray = new TrayApp();
            tray.ShowRequested += (_,__) => mainWindow?.Show();
            tray.SummaryRequested += async (_,__) => await mainWindow!.Dispatcher.InvokeAsync(async () => await mainWindow.RunSummaryAsyncWrapper());
            tray.PauseResumeRequested += (_,__) => mainWindow?.Dispatcher.Invoke(() => mainWindow.TriggerPauseResume());
            tray.ExitRequested += (_,__) => app.Shutdown();
            try
            {
                registrar = new GlobalHotkeyRegistrar(mainWindow, ModifierKeys.Control | ModifierKeys.Shift, 0x52); // R
                registrar.HotkeyPressed += async (_,__) => await mainWindow!.Dispatcher.InvokeAsync(async () => await mainWindow.RunSummaryAsyncWrapper());
            }
            catch (Exception ex)
            {
                tray.ShowBalloon("Hotkey", "No se pudo registrar hotkey global: " + ex.Message);
            }
        };
        app.Exit += (_, _) =>
        {
            registrar?.Dispose();
            tray?.Dispose();
        };
        app.Run();
    }

    private static void LoadDotEnv()
    {
        try
        {
            var dir = AppContext.BaseDirectory;
            string? path = null;
            for (int i = 0; i < 8 && path == null; i++)
            {
                var candidate = System.IO.Path.Combine(dir, ".env");
                if (System.IO.File.Exists(candidate)) path = candidate; else dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, ".."));
            }
            if (path == null) return;
            foreach (var raw in System.IO.File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
                var idx = line.IndexOf('=');
                if (idx <= 0) continue;
                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim();
                if (value.StartsWith("\"") && value.EndsWith("\"")) value = value[1..^1];
                if (key.Length == 0) continue;
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        catch { }
    }
}
