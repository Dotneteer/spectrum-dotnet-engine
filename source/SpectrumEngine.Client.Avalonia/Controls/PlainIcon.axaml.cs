using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class PlainIcon : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<PlainIcon, Brush>(nameof(Fill), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<Geometry> PathProperty = 
        AvaloniaProperty.Register<PlainIcon, Geometry>(nameof(Path));

    public PlainIcon()
    {
        Width = 24;
        Height = 24;
        Padding = new Thickness(4);
    }

    /// <summary>
    /// Fill brush for the button icon
    /// </summary>
    public Brush Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// The path property of the vector icon
    /// </summary>
    public Geometry Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

}