using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.ViewModels;

namespace SpectrumEngine.Client.Avalonia.Views;

public partial class DevToolsWindow : Window
{
    public DevToolsWindow()
    {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel; 

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DevToolsClosed(object? sender, EventArgs e)
    {
        if (Vm == null) return;
        Vm.DevTools.ShowDevTools = false;
        App.DiscardDevToolsWindow();
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (App.IsAppClosing || Vm == null) return;
        e.Cancel = true;

        await Task.Delay(100);
        Vm.DevTools.ShowDevTools = false;
        App.HideDevToolsWindow();
    }
}