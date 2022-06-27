using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class Sp48Key : UserControl
{
    public static readonly StyledProperty<SpectrumKeyCode> CodeProperty =
        AvaloniaProperty.Register<Sp48Key, SpectrumKeyCode>(nameof(Code));

    public static readonly StyledProperty<string> MainKeyProperty =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(MainKey));

    public static readonly StyledProperty<string> KeywordProperty =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(Keyword));

    public static readonly StyledProperty<string> Keyword2Property =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(Keyword2));

    public static readonly StyledProperty<string> SShiftKeyProperty =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(SShiftKey));

    public static readonly StyledProperty<string> ExtKeyProperty =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(ExtKey));

    public static readonly StyledProperty<string> ExtShiftKeyProperty =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(ExtShiftKey));

    public static readonly StyledProperty<string> ColorKeyProperty =
        AvaloniaProperty.Register<Sp48Key, string>(nameof(ColorKey));

    public static readonly StyledProperty<bool> SimpleModeProperty =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(SimpleMode));

    public static readonly StyledProperty<bool> NumericModeProperty =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(NumericMode));

    public static readonly StyledProperty<bool> SymModeProperty =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(SymMode));

    public static readonly StyledProperty<bool> TwoLineModeProperty =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(TwoLineMode));

    public static readonly StyledProperty<double> ButtonWidthProperty =
        AvaloniaProperty.Register<Sp48Key, double>(nameof(ButtonWidth));

    public static readonly StyledProperty<SolidColorBrush> NumForegroundProperty =
        AvaloniaProperty.Register<Sp48Key, SolidColorBrush>(nameof(NumForeground));

    public static readonly StyledProperty<SolidColorBrush> NumBackgroundProperty =
        AvaloniaProperty.Register<Sp48Key, SolidColorBrush>(nameof(NumBackground));

    public static readonly StyledProperty<bool> HasGraphicsProperty =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(HasGraphics));

    public static readonly StyledProperty<bool> HasBit0Property =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(HasBit0));

    public static readonly StyledProperty<bool> HasBit1Property =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(HasBit1));

    public static readonly StyledProperty<bool> HasBit2Property =
        AvaloniaProperty.Register<Sp48Key, bool>(nameof(HasBit2));

    public SpectrumKeyCode Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public string MainKey
    {
        get => GetValue(MainKeyProperty);
        set => SetValue(MainKeyProperty, value);
    }

    public string Keyword
    {
        get => GetValue(KeywordProperty);
        set => SetValue(KeywordProperty, value);
    }

    public string Keyword2
    {
        get => GetValue(Keyword2Property);
        set => SetValue(Keyword2Property, value);
    }

    public string SShiftKey
    {
        get => GetValue(SShiftKeyProperty);
        set => SetValue(SShiftKeyProperty, value);
    }

    public string ExtKey
    {
        get => GetValue(ExtKeyProperty);
        set => SetValue(ExtKeyProperty, value);
    }

    public string ExtShiftKey
    {
        get => GetValue(ExtShiftKeyProperty);
        set => SetValue(ExtShiftKeyProperty, value);
    }

    public string ColorKey
    {
        get => GetValue(ColorKeyProperty);
        set => SetValue(ColorKeyProperty, value);
    }

    public bool SimpleMode
    {
        get => GetValue(SimpleModeProperty);
        set => SetValue(SimpleModeProperty, value);
    }

    public bool NumericMode
    {
        get => GetValue(NumericModeProperty);
        set => SetValue(NumericModeProperty, value);
    }

    public bool SymMode
    {
        get => GetValue(SymModeProperty);
        set => SetValue(SymModeProperty, value);
    }

    public bool TwoLineMode
    {
        get => GetValue(TwoLineModeProperty);
        set => SetValue(TwoLineModeProperty, value);
    }

    public double ButtonWidth
    {
        get => GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    public SolidColorBrush NumForeground
    {
        get => GetValue(NumForegroundProperty);
        set => SetValue(NumForegroundProperty, value);
    }

    public SolidColorBrush NumBackground
    {
        get => GetValue(NumBackgroundProperty);
        set => SetValue(NumBackgroundProperty, value);
    }

    public bool HasGraphics
    {
        get => GetValue(HasGraphicsProperty);
        set => SetValue(HasGraphicsProperty, value);
    }

    public bool HasBit0
    {
        get => GetValue(HasBit0Property);
        set => SetValue(HasBit0Property, value);
    }

    public bool HasBit1
    {
        get => GetValue(HasBit1Property);
        set => SetValue(HasBit1Property, value);
    }

    public bool HasBit2
    {
        get => GetValue(HasBit2Property);
        set => SetValue(HasBit2Property, value);
    }

    public event EventHandler<PointerPressedEventArgs>? MainKeyClicked;

    public event EventHandler<PointerPressedEventArgs>? SymShiftKeyClicked;

    public event EventHandler<PointerPressedEventArgs>? ExtKeyClicked;

    public event EventHandler<PointerPressedEventArgs>? ExtShiftKeyClicked;

    public event EventHandler<PointerPressedEventArgs>? NumericControlKeyClicked;

    public event EventHandler<PointerPressedEventArgs>? GraphicsControlKeyClicked;

    public event EventHandler<PointerReleasedEventArgs>? KeyReleased;

    public SpectrumKeyCode? SecondaryCode { get; set; } = null;


    public Sp48Key()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnMainKeyMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
            control.Classes.Set("Pressed", true);
        }

        SecondaryCode = null;
        MainKeyClicked?.Invoke(this, e);
    }

    private void OnSymShiftKeyMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
            control.Classes.Set("Pressed", true);
        }

        SecondaryCode = null;
        SymShiftKeyClicked?.Invoke(this, e);
    }

    private void OnExtKeyMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
            control.Classes.Set("Pressed", true);
        }

        if (NumericMode)
        {
            SecondaryCode = SpectrumKeyCode.CShift;
            MainKeyClicked?.Invoke(this, e);
        }
        else
        {
            ExtKeyClicked?.Invoke(this, e);
        }
    }

    private void OnExtShiftKeyMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
            control.Classes.Set("Pressed", true);
        }

        SecondaryCode = null;
        ExtShiftKeyClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Forward the numeric control key clicked event
    /// </summary>
    private void OnNumericControlKeyMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
            control.Classes.Set("Pressed", true);
        }

        SecondaryCode = null;
        NumericControlKeyClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Handle the event when the graphics key has been clicked
    /// </summary>
    private void OnGraphicsKeyMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
            control.Classes.Set("Pressed", true);
        }

        GraphicsControlKeyClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Handle the event when the key has been released
    /// </summary>
    private void OnMouseUp(object sender, PointerReleasedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(null);
            control.Classes.Set("Pressed", false);
        }
        KeyReleased?.Invoke(this, e);

    }
}

/// <summary>
/// Design time sample data for Sp48KeyControl
/// </summary>
public class SingleKeyControlSampleData
{
    public string MainKey { get; set; } = "G";
    public string Keyword { get; set; } = "RETURN";
    public string Keyword2 { get; set; } = "RETURN";
    public string SShiftKey { get; set; } = "@";
    public string ExtKey { get; set; } = "READ";
    public string ExtShiftKey { get; set; } = "CIRCLE";
    public bool SimpleMode { get; set; } = false;
    public bool NumericMode { get; set; } = true;
    public bool HasGraphics { get; set; } = true;
    public int GraphicsCode { get; set; } = 7;
    public bool HasBit0 { get; set; } = true;
    public bool HasBit1 { get; set; } = true;
    public bool HasBit2 { get; set; } = true;
    public bool SymMode { get; set; } = false;
    public bool TwoLineMode { get; set; } = false;
    public double ButtonWidth { get; set; } = 100.0;
    public string ColorKey { get; set; } = "BLUE";
    public SolidColorBrush NumForeground { get; set; } = new (Colors.Blue);
    public SolidColorBrush NumBackground { get; set; } = new (Colors.Transparent);
}
