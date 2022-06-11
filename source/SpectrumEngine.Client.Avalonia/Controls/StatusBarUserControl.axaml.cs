using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class StatusBarUserControl : UserControl
{
    public StatusBarUserControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}