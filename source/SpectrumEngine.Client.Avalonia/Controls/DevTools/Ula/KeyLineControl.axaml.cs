using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// This control displays the state of a keyboard line
/// </summary>
public partial class KeyLineControl : UserControl
{
    public static readonly StyledProperty<string> RegProperty =
        AvaloniaProperty.Register<KeyLineControl, string>(nameof(Reg));

    public static readonly StyledProperty<byte> ValueProperty =
        AvaloniaProperty.Register<KeyLineControl, byte>(nameof(Value));

    public static readonly StyledProperty<string> CommentProperty =
        AvaloniaProperty.Register<KeyLineControl, string>(nameof(Comment));

    public KeyLineControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Keyboard line label
    /// </summary>
    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    /// <summary>
    /// Keyboard line value
    /// </summary>
    public ushort Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Keyboard line comment
    /// </summary>
    public string Comment
    {
        get => GetValue(CommentProperty);
        set => SetValue(CommentProperty, value);
    }
}