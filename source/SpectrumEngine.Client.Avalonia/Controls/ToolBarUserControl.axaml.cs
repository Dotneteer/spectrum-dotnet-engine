using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class ToolBarUserControl : UserControl
{
    public ToolBarUserControl()
    {
        InitializeComponent();
        DataContext = App.AppViewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}