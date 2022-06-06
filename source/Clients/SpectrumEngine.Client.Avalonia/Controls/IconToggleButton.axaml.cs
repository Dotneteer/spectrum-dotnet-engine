using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class IconToggleButton : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<IconToggleButton, Brush>(nameof(Fill), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<Geometry> PathProperty = 
        AvaloniaProperty.Register<IconToggleButton, Geometry>(nameof(Path));

    public static readonly StyledProperty<string> HintProperty =
        AvaloniaProperty.Register<IconToggleButton, string>(nameof(Hint));
    
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<IconToggleButton, bool>(nameof(IsChecked));
    
    public IconToggleButton()
    {
        Width = 32;
        Height = 32;
        Padding = new Thickness(6);
        Margin = new Thickness(0);
        Background = new SolidColorBrush(Colors.Transparent);
    }

    public Brush Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Geometry Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public string Hint
    {
        get => GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
}