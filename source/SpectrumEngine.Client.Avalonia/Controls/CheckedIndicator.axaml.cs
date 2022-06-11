using Avalonia;
using Avalonia.Controls.Primitives;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This control is a simple checkmark we use within a menu item to sign its checked state.
/// </summary>
public class CheckedIndicator : TemplatedControl
{
    public static readonly StyledProperty<bool> IsCheckedProperty = 
        AvaloniaProperty.Register<CheckedIndicator, bool>("IsChecked");

    /// <summary>
    /// SIgn if the menu item should display a check mark
    /// </summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
}