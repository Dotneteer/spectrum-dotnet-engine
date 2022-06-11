using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class MachineStatusOverlay : UserControl
{
    public MachineStatusOverlay()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}