using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

/// <summary>
/// This control represents a single flag's value with and icon 
/// </summary>
public partial class FlagControl : UserControl
{
    public static readonly StyledProperty<bool> ValueProperty =
        AvaloniaProperty.Register<FlagControl, bool>(nameof(Value));

    public FlagControl()
    {
        InitializeComponent();
    }

    public bool Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}