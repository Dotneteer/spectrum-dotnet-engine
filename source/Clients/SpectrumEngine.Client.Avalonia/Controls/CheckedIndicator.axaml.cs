using Avalonia;
using Avalonia.Controls.Primitives;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class CheckedIndicator : TemplatedControl
{
    public static readonly StyledProperty<bool> IsCheckedProperty = 
        AvaloniaProperty.Register<CheckedIndicator, bool>("IsChecked");

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
}