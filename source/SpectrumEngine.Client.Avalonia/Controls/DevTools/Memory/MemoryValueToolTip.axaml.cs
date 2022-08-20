using Avalonia;
using Avalonia.Controls.Primitives;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Memory;

public class MemoryValueToolTip : TemplatedControl
{
    public static readonly StyledProperty<ushort> ValueProperty = 
        AvaloniaProperty.Register<MemoryValueToolTip, ushort>(nameof(Value));

    public ushort Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}