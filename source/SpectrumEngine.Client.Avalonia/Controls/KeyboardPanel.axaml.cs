using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class KeyboardPanel : UserControl
{
    public KeyboardPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        if (change.Property.Name != "IsVisible") return;
        if (change.NewValue.Value is true)
        {
            this.GetLogicalDescendants().OfType<Sp48Key>().ToList().ForEach(key =>
            {
                key.MainKeyClicked += OnMainKeyClicked;
                key.SymShiftKeyClicked += OnSymShiftKeyClicked;
                key.ExtKeyClicked += OnExtKeyClicked;
                key.ExtShiftKeyClicked += OnExtShiftKeyClicked;
                key.NumericControlKeyClicked += OnNumericControlKeyClicked;
                key.GraphicsControlKeyClicked += OnGraphicsControlKeyClicked;
                key.KeyReleased += OnKeyReleased;
            });
        }
        else
        {
            this.GetLogicalDescendants().OfType<Sp48Key>().ToList().ForEach(key =>
            {
                key.MainKeyClicked -= OnMainKeyClicked;
                key.SymShiftKeyClicked -= OnSymShiftKeyClicked;
                key.ExtKeyClicked -= OnExtKeyClicked;
                key.ExtShiftKeyClicked -= OnExtShiftKeyClicked;
                key.NumericControlKeyClicked -= OnNumericControlKeyClicked;
                key.GraphicsControlKeyClicked -= OnGraphicsControlKeyClicked;
                key.KeyReleased -= OnKeyReleased;
            });
        }
    }

    private void OnMainKeyClicked(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnSymShiftKeyClicked(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnExtKeyClicked(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnExtShiftKeyClicked(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnNumericControlKeyClicked(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnGraphicsControlKeyClicked(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnKeyReleased(object? sender, PointerReleasedEventArgs e)
    {
    }
}