using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SpectrumEngine.Client.Avalonia.ViewModels;

// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class ViewsPanelUserControl : UserControl
{
    public ViewsPanelUserControl()
    {
        InitializeComponent();
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;


    private void OnMemoryPanelGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (Vm == null) return;
        Vm.MemoryViewer.MemoryPanelIsOnTop = true;
        MemoryPanel.SetPreferredTopPosition();
    }

    private void OnSelectedTabChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Vm == null) return;
        Vm.MemoryViewer.MemoryPanelIsOnTop =
            e.AddedItems.Count > 0
            && e.AddedItems[0] != null
            && e.AddedItems[0]!.Equals(MemoryTab);
    }
}