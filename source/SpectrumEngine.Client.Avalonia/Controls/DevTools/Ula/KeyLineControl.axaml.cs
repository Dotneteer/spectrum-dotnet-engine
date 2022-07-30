using Avalonia;
using Avalonia.Controls;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

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

    public string Reg
    {
        get => GetValue(RegProperty);
        set => SetValue(RegProperty, value);
    }

    public ushort Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Comment
    {
        get => GetValue(CommentProperty);
        set => SetValue(CommentProperty, value);
    }
}