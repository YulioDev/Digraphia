using Avalonia.Controls;
using Avalonia.Threading;
using Digrafos.Services;

namespace Digrafos.Views;

public partial class ConsoleOutputView : UserControl
{
    public ConsoleOutputView()
    {
        InitializeComponent();

        ConsoleService.MessageLogged += OnMessage;
        ConsoleService.StateChanged  += OnStateChanged;
    }

    private void OnMessage(string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            txtConsoleOutput.Text += "\n" + line;
        });
    }

    private void OnStateChanged(string text, ConsoleState mode)
    {
        Dispatcher.UIThread.Post(() =>
        {
            txtStateIndicator.Text = text;
            txtStateIndicator.Foreground = mode switch
            {
                ConsoleState.Ready   => Avalonia.Media.Brushes.LightGreen,
                ConsoleState.Warning => Avalonia.Media.Brush.Parse("#FFA500"),
                ConsoleState.Error   => Avalonia.Media.Brush.Parse("#FF4444"),
                _                   => Avalonia.Media.Brushes.LightGreen
            };
        });
    }
}