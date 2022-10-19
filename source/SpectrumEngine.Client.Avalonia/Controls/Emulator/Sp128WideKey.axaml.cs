using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.Emulator;

public partial class Sp128WideKey : UserControl
{
    public static readonly StyledProperty<double> ButtonWidthProperty =
        AvaloniaProperty.Register<Sp128WideKey, double>(nameof(ButtonWidth), 110.0);

    public static readonly StyledProperty<SpectrumKeyCode> CodeProperty =
        AvaloniaProperty.Register<Sp128WideKey, SpectrumKeyCode>(nameof(Code));

    public static readonly StyledProperty<SpectrumKeyCode> SecondaryCodeProperty =
        AvaloniaProperty.Register<Sp128WideKey, SpectrumKeyCode>(nameof(SecondaryCode));

    public static readonly StyledProperty<string> MainKeyProperty =
        AvaloniaProperty.Register<Sp128WideKey, string>(nameof(MainKey));

    public static readonly StyledProperty<string> KeywordProperty =
        AvaloniaProperty.Register<Sp128WideKey, string>(nameof(Keyword));

    public double ButtonWidth
    {
        get => GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

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

    public Sp128WideKey()
    {
        InitializeComponent();
        DataContext = this;
    }

    public event EventHandler<PointerPressedEventArgs>? MainKeyClicked;

    public event EventHandler<PointerReleasedEventArgs>? KeyReleased;

    private void OnPointerEnter(object? sender, PointerEventArgs e)
    {
        ButtonBack1.Classes.Set("MouseOver", true);
        ButtonBack2.Classes.Set("MouseOver", true);
        ButtonBack3.Classes.Set("MouseOver", true);
        e.Handled = true;
    }

    private void OnPointerLeave(object? sender, PointerEventArgs e)
    {
        ButtonBack1.Classes.Set("MouseOver", false);
        ButtonBack2.Classes.Set("MouseOver", false);
        ButtonBack3.Classes.Set("MouseOver", false);
        e.Handled = true;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            e.Pointer.Capture(control);
        }
        ButtonBack1.Classes.Set("Pressed", true);
        ButtonBack2.Classes.Set("Pressed", true);
        ButtonBack3.Classes.Set("Pressed", true);
        e.Handled = true;
        MainKeyClicked?.Invoke(this, e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        ButtonBack1.Classes.Set("Pressed", false);
        ButtonBack2.Classes.Set("Pressed", false);
        ButtonBack3.Classes.Set("Pressed", false);
        e.Handled = true;
        KeyReleased?.Invoke(this, e);
    }
}