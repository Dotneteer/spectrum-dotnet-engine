using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

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

    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    public byte Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}