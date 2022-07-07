using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class EmulatorToolBarUserControl : UserControl
{
    public EmulatorToolBarUserControl()
    {
        InitializeComponent();
        DataContext = App.AppViewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}