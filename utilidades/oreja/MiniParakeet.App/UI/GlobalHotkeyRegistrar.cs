using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;

namespace MiniParakeet.App.UI;

public sealed class GlobalHotkeyRegistrar : IDisposable
{
    private readonly Window _window;
    private readonly int _id;
    public event EventHandler? HotkeyPressed;

    public GlobalHotkeyRegistrar(Window window, ModifierKeys modifiers, int vk)
    {
        _window = window;
        _id = GetHashCode();
        var source = (HwndSource)PresentationSource.FromVisual(window)!;
        source.AddHook(WndProc);
        if (!RegisterHotKey(source.Handle, _id, (uint)modifiers, (uint)vk))
            throw new InvalidOperationException("No se pudo registrar hotkey global");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        var source = (HwndSource)PresentationSource.FromVisual(_window)!;
        UnregisterHotKey(source.Handle, _id);
    }

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

[Flags]
public enum ModifierKeys : uint
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}
