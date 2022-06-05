using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class IconButton : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<IconButton, Brush>("Fill", new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<Geometry> PathProperty = 
        AvaloniaProperty.Register<IconButton, Geometry>("Path");

    public static readonly StyledProperty<string> HintProperty =
        AvaloniaProperty.Register<IconButton, string>("Hint");
        
    public IconButton()
    {
        Width = 28;
        Height = 28;
        Padding = new Thickness(4);
        Margin = new Thickness(2);
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
}