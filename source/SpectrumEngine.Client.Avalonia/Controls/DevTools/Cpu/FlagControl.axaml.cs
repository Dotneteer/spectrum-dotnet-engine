using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Cpu;

public partial class FlagControl : UserControl
{
    public static readonly StyledProperty<bool> ValueProperty =
        AvaloniaProperty.Register<FlagControl, bool>(nameof(Value));

    public FlagControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public bool Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}