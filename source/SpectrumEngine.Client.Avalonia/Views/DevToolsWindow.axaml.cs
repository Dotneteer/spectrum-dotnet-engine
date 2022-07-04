using System;
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
}