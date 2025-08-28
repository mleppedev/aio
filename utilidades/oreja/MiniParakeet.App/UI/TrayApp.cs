using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniParakeet.App.UI;

public sealed class TrayApp : IDisposable
{
    private readonly NotifyIcon _notify;
    public event EventHandler? ShowRequested;
    public event EventHandler? SummaryRequested;
    public event EventHandler? PauseResumeRequested;
    public event EventHandler? ExitRequested;

    public TrayApp()
    {
        _notify = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Visible = true,
            Text = "Mini Parakeet"
        };
        var menu = new ContextMenuStrip();
        menu.Items.Add("Mostrar", null, (_,__) => ShowRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Resumen", null, (_,__) => SummaryRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Pausa/Reanudar", null, (_,__) => PauseResumeRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Salir", null, (_,__) => ExitRequested?.Invoke(this, EventArgs.Empty));
        _notify.ContextMenuStrip = menu;
        _notify.DoubleClick += (_,__) => ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ShowBalloon(string title, string message) => _notify.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);

    public void Dispose()
    {
        _notify.Visible = false;
        _notify.Dispose();
    }
}
