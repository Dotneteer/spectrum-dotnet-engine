using Avalonia;
using Avalonia.Controls;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public abstract class MachineStatusUserControl: UserControl
{
    private int _counter;
    
    public static readonly StyledProperty<int> RefreshRateProperty =
        AvaloniaProperty.Register<MachineStatusUserControl, int>(nameof(RefreshRate), 5);

    public static readonly StyledProperty<MachineController> ControllerProperty =
        AvaloniaProperty.Register<MachineStatusUserControl, MachineController>(nameof(Controller));

    public int RefreshRate
    {
        get => GetValue(RefreshRateProperty);
        set => SetValue(RefreshRateProperty, value);
    }

    public MachineController Controller
    {
        get => GetValue(ControllerProperty);
        set => SetValue(ControllerProperty, value);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        if (change.Property != ControllerProperty) return;
        
        // --- Release the events of the old controller
        if (change.OldValue.HasValue && change.OldValue.Value is MachineController oldController)
        {
            oldController.FrameCompleted -= OnFrameCompleted;
            oldController.StateChanged -= OnStateChanged;
        }
        
        if (!change.NewValue.HasValue || change.NewValue.Value is not MachineController newController) return;
        
        // --- Setup the events of the new controller
        newController.FrameCompleted += OnFrameCompleted;
        newController.StateChanged += OnStateChanged;
        NewControllerSet(newController);
    }

    protected abstract void NewControllerSet(MachineController controller);

    protected abstract void RefreshPanel();
    
    private void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        _counter = 0;
        RefreshPanel();
    }

    private void OnFrameCompleted(object? sender, bool e)
    {
        _counter++;
        if (RefreshRate <= 0 || _counter % RefreshRate == 0)
        {
            RefreshPanel();
        }
    }
}