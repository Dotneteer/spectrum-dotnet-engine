using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class StatusLabel : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<StatusLabel, Brush>(nameof(Fill), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<Geometry> PathProperty = 
        AvaloniaProperty.Register<StatusLabel, Geometry>(nameof(Path));

    public static readonly StyledProperty<string> TextProperty = 
        AvaloniaProperty.Register<StatusLabel, string>(nameof(Text));

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

    /// <summary>
    /// Label text to display
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}