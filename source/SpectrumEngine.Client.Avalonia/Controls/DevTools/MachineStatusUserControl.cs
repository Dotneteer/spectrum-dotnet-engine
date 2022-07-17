using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public abstract class MachineStatusUserControl: UserControl
{
    private int _counter;
    
    public static readonly StyledProperty<int> RefreshRateProperty =
        AvaloniaProperty.Register<MachineStatusUserControl, int>(nameof(RefreshRate), 5);

    public int RefreshRate
    {
        get => GetValue(RefreshRateProperty);
        set => SetValue(RefreshRateProperty, value);
    }

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
        vm.Machine.ControllerChanged += (sender, tuple) =>
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

    protected abstract void RefreshPanel();
    
    private void OnStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _counter = 0;
            RefreshPanel();
        });
    }

    private void OnFrameCompleted(object? sender, bool e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _counter++;
            if (RefreshRate <= 0 || _counter % RefreshRate == 0)
            {
                RefreshPanel();
            }
        });
    }
}