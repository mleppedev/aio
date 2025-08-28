using System.Windows;
using System.Windows.Input;

namespace MiniParakeet.App.UI;

public partial class ResponseOverlay : Window
{
    public ResponseOverlay()
    {
        InitializeComponent();
    }

    public void SetText(string text) => ContentText.Text = text;

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape) Close();
    }

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}
