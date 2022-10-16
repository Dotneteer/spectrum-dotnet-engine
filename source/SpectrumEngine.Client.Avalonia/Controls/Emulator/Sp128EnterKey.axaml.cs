using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace SpectrumEngine.Client.Avalonia.Controls.Emulator;

public partial class Sp128EnterKey : UserControl
{
    public Sp128EnterKey()
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
        ButtonBack4.Classes.Set("MouseOver", true);
        ButtonBack5.Classes.Set("MouseOver", true);
        e.Handled = true;
    }

    private void OnPointerLeave(object? sender, PointerEventArgs e)
    {
        ButtonBack1.Classes.Set("MouseOver", false);
        ButtonBack2.Classes.Set("MouseOver", false);
        ButtonBack3.Classes.Set("MouseOver", false);
        ButtonBack4.Classes.Set("MouseOver", false);
        ButtonBack5.Classes.Set("MouseOver", false);
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
        ButtonBack4.Classes.Set("Pressed", true);
        ButtonBack5.Classes.Set("Pressed", true);
        e.Handled = true;
        MainKeyClicked?.Invoke(this, e);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        ButtonBack1.Classes.Set("Pressed", false);
        ButtonBack2.Classes.Set("Pressed", false);
        ButtonBack3.Classes.Set("Pressed", false);
        ButtonBack4.Classes.Set("Pressed", false);
        ButtonBack5.Classes.Set("Pressed", false);
        e.Handled = true;
        KeyReleased?.Invoke(this, e);
    }
}