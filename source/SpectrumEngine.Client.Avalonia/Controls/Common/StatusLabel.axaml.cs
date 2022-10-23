using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This control implements a status label (used in a status bar)
/// </summary>
public class StatusLabel : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<StatusLabel, Brush>(nameof(Fill), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<string> IconNameProperty = 
        AvaloniaProperty.Register<StatusLabel, string>(nameof(IconName));

    public static readonly StyledProperty<string> TextProperty = 
        AvaloniaProperty.Register<StatusLabel, string>(nameof(Text));

    public static readonly StyledProperty<bool> ShowIconProperty = 
        AvaloniaProperty.Register<StatusLabel, bool>(nameof(ShowIcon), true);

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

    /// <summary>
    /// Label text to display
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Indicates if icon is visible
    /// </summary>
    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }
}