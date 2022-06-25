using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class Sp48Key : TemplatedControl
{
    public static readonly StyledProperty<SpectrumKeyCode> CodeProperty = 
        AvaloniaProperty.Register<Sp48Key, SpectrumKeyCode>(nameof(Code), SpectrumKeyCode.A);

    public static readonly StyledProperty<string> MainKeyProperty = 
        AvaloniaProperty.Register<Sp48Key, string>(nameof(MainKey), "NEW");

    public SpectrumKeyCode Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public string MainKey
    {
        get => GetValue(MainKeyProperty);
        set => SetValue(MainKeyProperty, value);
    }
}