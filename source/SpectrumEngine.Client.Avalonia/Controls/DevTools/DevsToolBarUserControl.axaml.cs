using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class DevsToolBarUserControl : UserControl
{
    public DevsToolBarUserControl()
    {
        InitializeComponent();
        DataContext = App.AppViewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}