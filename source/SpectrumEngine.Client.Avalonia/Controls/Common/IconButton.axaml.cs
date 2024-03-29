using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This control represents a button with a single icon definid via the Path property
/// </summary>
public class IconButton : TemplatedControl
{
    public static readonly StyledProperty<Brush> FillProperty = 
        AvaloniaProperty.Register<IconButton, Brush>(nameof(Fill), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<Geometry> PathProperty = 
        AvaloniaProperty.Register<IconButton, Geometry>(nameof(Path));

    public static readonly StyledProperty<string> HintProperty =
        AvaloniaProperty.Register<IconButton, string>(nameof(Hint));
    
    public static readonly StyledProperty<ICommand?> CommandProperty = 
        AvaloniaProperty.Register<IconButton, ICommand?>(nameof(Command));
   
    public static readonly StyledProperty<object?> CommandParameterProperty = 
        AvaloniaProperty.Register<IconButton, object?>(nameof(CommandParameter));
    
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(IsChecked));
    
    public static readonly StyledProperty<bool> IsDisabledProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(IsDisabledProperty));

    /// <summary>
    /// HACK: We need this method to display an extra border so we can display the tooltip when the underlying button
    /// is disabled.
    /// </summary>
    protected override void OnPointerEnter(PointerEventArgs e)
    {
        IsDisabled = !(Command?.CanExecute(null) ?? true);
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

    /// <summary>
    /// Hint to use with the icon button (not used yet)
    /// </summary>
    public string Hint
    {
        get => GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    /// <summary>
    /// Command to execute when the button has been clicked
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Parameter of the command to execute
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Indicates if the button is checked
    /// </summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public bool IsDisabled
    {
        get => GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }
}