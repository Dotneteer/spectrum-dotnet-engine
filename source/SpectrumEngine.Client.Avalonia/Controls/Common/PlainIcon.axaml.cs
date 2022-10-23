using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This control displays a simple icon given with an SVG path
/// </summary>
public class PlainIcon : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<PlainIcon, Brush>(nameof(Fill), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<string> IconNameProperty = 
        AvaloniaProperty.Register<PlainIcon, string>(nameof(IconName));

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
    public string IconName
    {
        get => GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }
}