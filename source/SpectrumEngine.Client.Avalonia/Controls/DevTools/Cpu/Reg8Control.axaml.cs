using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

/// <summary>
/// This control displays the contents of an 8-bit register
/// </summary>
public partial class Reg8Control : UserControl
{
    public static readonly StyledProperty<string> RegProperty =
        AvaloniaProperty.Register<Reg8Control, string>(nameof(Reg));

    public static readonly StyledProperty<byte> ValueProperty =
        AvaloniaProperty.Register<Reg8Control, byte>(nameof(Value));

    public Reg8Control()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Reginster label
    /// </summary>
    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    /// <summary>
    /// Register value
    /// </summary>
    public byte Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}