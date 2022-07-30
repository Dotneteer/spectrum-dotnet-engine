using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

/// <summary>
/// This control displays the contents of a 16-bit register
/// </summary>
public partial class Reg16Control : UserControl
{
    public static readonly StyledProperty<string> RegProperty =
        AvaloniaProperty.Register<Reg16Control, string>(nameof(Reg));

    public static readonly StyledProperty<ushort> ValueProperty =
        AvaloniaProperty.Register<Reg16Control, ushort>(nameof(Value));

    public Reg16Control()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Register label
    /// </summary>
    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    /// <summary>
    /// Register value
    /// </summary>
    public ushort Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}