using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// This control is the base class for all other controls (information panels) that depend on the status of the
/// emulated machine.
/// </summary>
public abstract class MachineStatusUserControl: UserControl
{
    private int _counter;
    
    public static readonly StyledProperty<int> RefreshRateProperty =
        AvaloniaProperty.Register<MachineStatusUserControl, int>(nameof(RefreshRate), 5);

    /// <summary>
    /// Refresh rate of the information, given in frame counts
    /// </summary>
    public int RefreshRate
    {
        get => GetValue(RefreshRateProperty);
        set => SetValue(RefreshRateProperty, value);
    }

    /// <summary>
    /// Bind this control to the machine events when the data context changes. 
    /// </summary>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not MainWindowViewModel vm) return;

        if (vm.Machine.Controller != null)
        {
            vm.Machine.Controller.FrameCompleted += OnFrameCompleted;
            vm.Machine.Controller.StateChanged += OnStateChanged;
        }
        
        // --- Respond to machine controller changes
        vm.Machine.ControllerChanged += (_, tuple) =>
        {
            var (oldController, newController) = tuple;
            if (oldController != null)
            {
                oldController.FrameCompleted -= OnFrameCompleted;
                oldController.StateChanged -= OnStateChanged;
            }

            if (newController == null) return;
            
            newController.FrameCompleted += OnFrameCompleted;
            newController.StateChanged += OnStateChanged;
        };
    }

    /// <summary>
    /// Override this event to refresh the contents of the information panel
    /// </summary>
    protected virtual void Refresh()
    {
        // --- Add some task in derived classes
    }

    protected virtual void RefreshOnStateChanged()
    {
        // --- Add some task in derived classes
    }

    protected virtual void RefreshOnFrameCompleted()
    {
        // --- Add some task in derived classes
    }
    
    /// <summary>
    /// Refresh the panel state whenever the machine's state changes
    /// </summary>
    private void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _counter = 0;
            RefreshOnStateChanged();
            Refresh();
        });
    }

    /// <summary>
    /// Refresh the panel state when the specified number of frames are displayed
    /// </summary>
    private void OnFrameCompleted(object? sender, bool e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _counter++;
            if (RefreshRate > 0 && _counter % RefreshRate != 0) return;
            RefreshOnFrameCompleted();
            Refresh();
        });
    }
}