using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

/// <summary>
/// This controle represents a singled flag with a label name and an icon
/// </summary>
public partial class LabeledFlagControl : UserControl
{
    public static readonly StyledProperty<string> RegProperty =
        AvaloniaProperty.Register<LabeledFlagControl, string>(nameof(Reg));

    public static readonly StyledProperty<bool> ValueProperty =
        AvaloniaProperty.Register<LabeledFlagControl, bool>(nameof(Value));

    public LabeledFlagControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Flag label
    /// </summary>
    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    /// <summary>
    /// Flag value
    /// </summary>
    public bool Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}