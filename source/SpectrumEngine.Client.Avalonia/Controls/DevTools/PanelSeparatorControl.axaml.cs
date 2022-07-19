using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class PanelSeparatorControl : UserControl
{
    public PanelSeparatorControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}