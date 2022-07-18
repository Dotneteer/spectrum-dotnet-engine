using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

public partial class StateValueControl : UserControl
{
    public static readonly StyledProperty<string> RegProperty =
        AvaloniaProperty.Register<StateValueControl, string>(nameof(Reg));

    public static readonly StyledProperty<object> ValueProperty =
        AvaloniaProperty.Register<StateValueControl, object>(nameof(Value));

    public StateValueControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}