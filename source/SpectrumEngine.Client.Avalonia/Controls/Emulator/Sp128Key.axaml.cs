using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.Emulator;

public partial class Sp128Key : UserControl
{
    public static readonly StyledProperty<SpectrumKeyCode> CodeProperty =
        AvaloniaProperty.Register<Sp128Key, SpectrumKeyCode>(nameof(Code));

    public static readonly StyledProperty<SpectrumKeyCode> SecondaryCodeProperty =
        AvaloniaProperty.Register<Sp128Key, SpectrumKeyCode>(nameof(SecondaryCode));

    public static readonly StyledProperty<string> MainKeyProperty =
        AvaloniaProperty.Register<Sp128Key, string>(nameof(MainKey));

    public static readonly StyledProperty<string> KeywordProperty =
        AvaloniaProperty.Register<Sp128Key, string>(nameof(Keyword));

    public static readonly StyledProperty<string> SShiftKeyProperty =
        AvaloniaProperty.Register<Sp128Key, string>(nameof(SShiftKey));

    public static readonly StyledProperty<string> ExtKeyProperty =
        AvaloniaProperty.Register<Sp128Key, string>(nameof(ExtKey));

    public static readonly StyledProperty<string> ExtShiftKeyProperty =
        AvaloniaProperty.Register<Sp128Key, string>(nameof(ExtShiftKey));

    public static readonly StyledProperty<bool> SimpleModeProperty =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(SimpleMode));

    public static readonly StyledProperty<bool> CleanModeProperty =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(CleanMode));

    public static readonly StyledProperty<bool> NumericModeProperty =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(NumericMode));

    public static readonly StyledProperty<bool> CenteredProperty =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(Centered));

    public static readonly StyledProperty<bool> HasGraphicsProperty =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(HasGraphics));

    public static readonly StyledProperty<bool> HasBit0Property =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(HasBit0));

    public static readonly StyledProperty<bool> HasBit1Property =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(HasBit1));

    public static readonly StyledProperty<bool> HasBit2Property =
        AvaloniaProperty.Register<Sp128Key, bool>(nameof(HasBit2));



    public SpectrumKeyCode Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public SpectrumKeyCode SecondaryCode
    {
        get => GetValue(SecondaryCodeProperty);
        set => SetValue(SecondaryCodeProperty, value);
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

    public bool SimpleMode
    {
        get => GetValue(SimpleModeProperty);
        set => SetValue(SimpleModeProperty, value);
    }

    public bool CleanMode
    {
        get => GetValue(CleanModeProperty);
        set => SetValue(CleanModeProperty, value);
    }

    public bool NumericMode
    {
        get => GetValue(NumericModeProperty);
        set => SetValue(NumericModeProperty, value);
    }

    public bool Centered
    {
        get => GetValue(CenteredProperty);
        set => SetValue(CenteredProperty, value);
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

    public Sp128Key()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }


    private void OnExtKeyMouseDown(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnMouseUp(object? sender, PointerReleasedEventArgs e)
    {
    }

    private void OnExtShifKeyMouseDown(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnMouseEnter(object? sender, PointerEventArgs e)
    {
    }

    private void OnMouseLeave(object? sender, PointerEventArgs e)
    {
    }

    private void OnMainKeyMouseDown(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnSShiftMouseDown(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnGraphicsKeyMouseDown(object? sender, PointerPressedEventArgs e)
    {
    }
}

/// <summary>
/// Design time sample data for Sp48KeyControl
/// </summary>
public class SingleKey128ControlSampleData
{
    public string MainKey { get; set; } = "G";
    public string Keyword { get; set; } = "RETURN";
    public string SShiftKey { get; set; } = "@";
    public string ExtKey { get; set; } = "READ";
    public string ExtShiftKey { get; set; } = "CIRCLE";
    public bool SimpleMode { get; set; } = false;
    public bool CleanMode { get; set; } = false;
    public bool NumericMode { get; set; } = true;
    public bool Centered { get; set; } = true;
    public bool HasGraphics { get; set; } = true;
    public bool HasBit0 { get; set; } = true;
    public bool HasBit1 { get; set; } = true;
    public bool HasBit2 { get; set; } = true;
    public bool HidesExtCaption { get; set; } = false;
}