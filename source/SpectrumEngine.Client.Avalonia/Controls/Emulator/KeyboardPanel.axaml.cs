using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class KeyboardPanel : UserControl
{
    // --- Stores the last pressed secondary button's key code
    private SpectrumKeyCode? _lastSecondary;
    private int _lastPressFrame;
    
    public KeyboardPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel; 

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
        if (sender is not Sp48Key key) return;
        
        _lastSecondary = key.SecondaryCode 
            ?? (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ? null : SpectrumKeyCode.CShift);
        SetKeyStatus(true, key.Code, _lastSecondary);
    }

    private void OnSymShiftKeyClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Sp48Key key) return;
        
        _lastSecondary = SpectrumKeyCode.SShift;
        SetKeyStatus(true, key.Code, _lastSecondary);
        e.Handled = true;
    }

    private void OnExtKeyClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Sp48Key key) return;
        if (!WaitForNextKeyPress(9)) return;
        
        QueueKey(0, 2, SpectrumKeyCode.CShift, SpectrumKeyCode.SShift);
        QueueKey(3, 2, key.Code, 
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ? null : SpectrumKeyCode.CShift);
        e.Handled = true;
    }

    private void OnExtShiftKeyClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Sp48Key key) return;
        if (!WaitForNextKeyPress(9)) return;

        QueueKey(0, 2, SpectrumKeyCode.CShift, SpectrumKeyCode.SShift);
        QueueKey(3, 2, key.Code, SpectrumKeyCode.SShift);
        e.Handled = true;
    }

    private void OnNumericControlKeyClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Sp48Key key) return;
        if (!WaitForNextKeyPress(9)) return;

        QueueKey(0, 2, SpectrumKeyCode.CShift, SpectrumKeyCode.SShift);
        QueueKey(3, 2, key.Code, 
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ? null : SpectrumKeyCode.CShift);
        e.Handled = true;
    }

    private void OnGraphicsControlKeyClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Sp48Key key) return;
        if (!WaitForNextKeyPress(12)) return;
        
        QueueKey(0, 2, SpectrumKeyCode.N9, SpectrumKeyCode.CShift);
        QueueKey(3, 2, key.Code, 
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ? null : SpectrumKeyCode.CShift);
        QueueKey(6, 2, SpectrumKeyCode.N9, SpectrumKeyCode.CShift);
    }

    private void OnKeyReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Sp48Key key) return;
        SetKeyStatus(false, key.Code, _lastSecondary);
        e.Handled = true;
    }

    /// <summary>
    /// Emulates pressing down or releasing a primary and an optional secondary key
    /// </summary>
    /// <param name="down">Is the key pressed down?</param>
    /// <param name="primary">Primary key code</param>
    /// <param name="secondary">Optional secondary key code</param>
    private void SetKeyStatus(bool down, SpectrumKeyCode primary, SpectrumKeyCode? secondary)
    {
        var machine = Vm?.Machine.Controller?.Machine;
        if (machine == null) return;
        
        machine.SetKeyStatus(primary, down);
        if (secondary.HasValue)
        {
            machine.SetKeyStatus(secondary.Value, down);
        }
        if (down) _lastPressFrame = machine.Frames;
    }

    /// <summary>
    /// Queues an emulated key press
    /// </summary>
    /// <param name="relativeStart">Realtive start (in frames) from the current frame</param>
    /// <param name="frames">Number of frames to keep the key pressed</param>
    /// <param name="primary">Primary key code</param>
    /// <param name="secondary">Optional secondary key code</param>
    private void QueueKey(int relativeStart, int frames, SpectrumKeyCode primary, SpectrumKeyCode? secondary)
    {
        var machine = Vm?.Machine.Controller?.Machine;
        if (machine == null) return;
        machine.QueueKeystroke(machine.Frames + relativeStart, frames, primary, secondary);
        _lastPressFrame = machine.Frames;
    }

    /// <summary>
    /// Waits the specified number of frames before allows a new key press
    /// </summary>
    /// <param name="frames">Number of frames to wait</param>
    /// <returns>True, if the next keypress is allowed; otherwise, false.</returns>
    private bool WaitForNextKeyPress(int frames)
    {
        var machine = Vm?.Machine.Controller?.Machine;
        return machine != null && machine.Frames > _lastPressFrame + frames;
    }
}