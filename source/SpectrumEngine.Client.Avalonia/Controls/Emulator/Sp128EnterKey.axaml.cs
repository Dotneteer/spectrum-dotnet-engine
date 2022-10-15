using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls.Emulator;

public partial class Sp128EnterKey : UserControl
{
    public Sp128EnterKey()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnPointerEnter(object? sender, PointerEventArgs e)
    {
    }

    private void OnPointerLeave(object? sender, PointerEventArgs e)
    {
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }
}