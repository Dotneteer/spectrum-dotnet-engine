using System;
using System.Threading.Tasks;
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
    private bool _isRefreshing;
    
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
    /// Prepares the refresh in the background thread
    /// </summary>
    protected virtual Task PrepareRefresh()
    {
        // --- Add some task in derived classes
        return Task.FromResult(0);
    }
    
    /// <summary>
    /// Override this event to refresh the contents of the information panel
    /// </summary>
    protected virtual Task Refresh()
    {
        // --- Add some task in derived classes
        return Task.FromResult(0);
    }

    protected virtual Task RefreshOnStateChanged()
    {
        // --- Add some task in derived classes
        return Task.FromResult(0);
    }

    protected virtual Task RefreshOnFrameCompleted()
    {
        // --- Add some task in derived classes
        return Task.FromResult(0);
    }

    /// <summary>
    /// Refresh the panel state whenever the machine's state changes
    /// </summary>
    private async void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        await PrepareRefresh();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_isRefreshing) return;
            _isRefreshing = true;
            try
            {
                _counter = 0;
                RefreshOnStateChanged();
                Refresh();
            }
            finally
            {
                _isRefreshing = false;
            }
        });
    }

    /// <summary>
    /// Refresh the panel state when the specified number of frames are displayed
    /// </summary>
    private async void OnFrameCompleted(object? sender, bool e)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_isRefreshing) return;
            _isRefreshing = true;
            try
            {
                _counter++;
                if (RefreshRate > 0 && _counter % RefreshRate != 0) return;
                await RefreshOnFrameCompleted();
                await Refresh();
            }
            finally
            {
                _isRefreshing = false;
            }
        });
    }
}