using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.ViewModels;
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class SiteBar : UserControl
{
    public SiteBar()
    {
        InitializeComponent();
    }

    private DevToolsViewModel? Vm => DataContext as DevToolsViewModel; 

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCpuPropertyChanged(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "IsExpanded" || Vm is null) return;
        Vm.SiteBar.CpuExpanded = e.NewValue != null && (bool)e.NewValue;
    }

    private void OnUlaPropertyChanged(object? _, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "IsExpanded" || Vm is null) return;
        Vm.SiteBar.UlaExpanded = e.NewValue != null && (bool)e.NewValue;
    }

    private void OnBreakpointsPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "IsExpanded" || Vm is null) return;
        Vm.SiteBar.BreakpointsExpanded = e.NewValue != null && (bool)e.NewValue;
    }
}