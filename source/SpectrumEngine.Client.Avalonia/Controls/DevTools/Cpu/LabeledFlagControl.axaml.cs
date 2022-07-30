using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

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

    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    public bool Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}