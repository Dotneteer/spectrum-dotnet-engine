using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SpectrumEngine.Client.Avalonia.Controls;

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
    
    public static readonly StyledProperty<bool> CanToggleProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(CanToggle));
    
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(IsChecked));
    
    public IconButton()
    {
        Width = 36;
        Height = 36;
        Padding = new Thickness(8);
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

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool CanToggle
    {
        get => GetValue(CanToggleProperty);
        set => SetValue(CanToggleProperty, value);
    }
    
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
}