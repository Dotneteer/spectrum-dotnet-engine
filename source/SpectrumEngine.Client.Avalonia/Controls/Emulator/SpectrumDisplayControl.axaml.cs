using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This control represents the image that displays the ZX Spectrum screen
/// </summary>
public partial class SpectrumDisplayControl : UserControl
{
    private object? _prevDataContext;
    private int _zoomFactor = 1;
    private IAudioProvider? _audioProvider;

    public SpectrumDisplayControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handle the keyboard event
    /// </summary>
    /// <remarks>
    /// This method maps a physical key to one or two ZX Spectrum keys and sets the key states through the keyboard
    /// device of the emulated machine.
    /// </remarks>
    public bool HandleKeyboardEvent(KeyEventArgs e, bool isPressed)
    {
        if (DataContext is not MainWindowViewModel context || context.Machine.Controller?.Machine == null)
        {
            return false;
        }

        var machine = context.Machine.Controller.Machine;
        var keyMapping = KeyMappings.DefaultMapping.FirstOrDefault(m => m.Input == e.Key);
        if (keyMapping == null) return false;
        
        // --- Apply the pressed key
        machine.SetKeyStatus(keyMapping.Primary, isPressed);
        if (keyMapping.Secondary != null)
        {
            machine.SetKeyStatus(keyMapping.Secondary.Value, isPressed);
        }
        return true;
    }

    /// <summary>
    /// Gets the current view model in the data context
    /// </summary>
    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;
  
    /// <summary>
    /// Change the view model properties when the user control is resized.
    /// </summary>
    /// <remarks>
    /// Display the user control in the largest integer zoom factor that fits to the screen.
    /// </remarks>
    private void ResizeScreen()
    {
        if (DataContext is not MainWindowViewModel context || context.Machine.Controller?.Machine == null)
        {
            return ;
        }

        var machine = context.Machine.Controller.Machine;
        var horZoom = (int)(Bounds.Width / machine.ScreenWidthInPixels);
        var vertZoom = (int)(Bounds.Height / machine.ScreenHeightInPixels);
        _zoomFactor = context.Display.ZoomFactor = Math.Max(1, Math.Min(horZoom, vertZoom));
        Display.Width = context.Display.ScreenWidth = _zoomFactor * machine.ScreenWidthInPixels;
        Display.Height = context.Display.ScreenHeight = _zoomFactor * machine.ScreenHeightInPixels;
    }

    /// <summary>
    /// Respond to the machine controller's state changes
    /// </summary>
    private void OnControllerStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        if (Vm?.Machine.Controller == null) return;

        var display = Vm.Display;
        display.IsDebugging = Vm.Machine.Controller.IsDebugging;
        Vm.Cpu?.SignStateChanged();
        Vm.Machine.RaisePropertyChanged(nameof(Vm.Machine.Controller));

        switch (e.NewState)
        {
            case MachineControllerState.None:
                display.OverlayMessage = "Start the machine";
                break;
            case MachineControllerState.Running:
                display.OverlayMessage = display.IsDebugging ? "Debug mode" : "Running";
                _audioProvider?.Play();
                break;
            case MachineControllerState.Paused:
                display.OverlayMessage = "Paused";
                _audioProvider?.Pause();
                break;
            case MachineControllerState.Stopped:
                display.OverlayMessage = "Stopped";
                _audioProvider?.Stop();
                break;
        }
    }

    private void OnControllerFrameCompleted(object? sender, bool e)
    {
        var machine = (sender as MachineController)?.Machine;
        if (machine == null) return;

        // --- Add sound samples
        var samples = (machine as ZxSpectrum48Machine)?.BeeperDevice.GetAudioSamples();
        if (samples != null)
        {
            _audioProvider?.AddSamples(samples);
        }

        // --- Display the new screen frame
        var bitmap = new WriteableBitmap(
            new PixelSize(machine.ScreenWidthInPixels * _zoomFactor, machine.ScreenHeightInPixels * _zoomFactor),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
        var width = machine.ScreenWidthInPixels;
        var height = machine.ScreenHeightInPixels;

        using var bitmapBuffer = bitmap.Lock();
        unsafe
        {
            var buffer = machine.GetPixelBuffer();
            var pBackBuffer = bitmapBuffer.Address;
            var offset = width;
            for (var y = 0; y < height; y++)
            {
                var rowStart = offset;
                for (var rowRepeat = 0; rowRepeat < _zoomFactor; rowRepeat++)
                {
                    offset = rowStart;
                    for (var x = 0; x < width; x++)
                    {
                        for (var colRepeat = 0; colRepeat < _zoomFactor; colRepeat++)
                        {
                            *(uint*)pBackBuffer = buffer[offset];
                            pBackBuffer += 4;
                        }
                        offset++;
                    }
                }
            }
        }

        // --- Use the Dispatcher to render the screen        
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Display.Source = bitmap;

            // --- Refresh status information
            if (machine.Frames % 10 == 0)
            {
                Vm?.Cpu?.SignStateChanged();
            }
        });
    }

    private void OnMachinePropertyChanged(object? sender, (string key, object? value) args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (args.key != MachinePropNames.TapeMode || args.value is not TapeMode tapeMode) return;
            if (Vm?.Machine.Controller == null) return;

            var display = Vm.Display;
            switch (tapeMode)
            {
                case TapeMode.Load:
                    display.OverlayMessage = "LOAD mode";
                    break;
                        
                case TapeMode.Save:
                    display.OverlayMessage = "SAVE mode";
                    break;
            }
            Vm?.RaisePropertyChanged(nameof(Vm.Machine.Controller));
        });
    }   

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // --- Release the events of the old controller
        var oldVm = _prevDataContext as MainWindowViewModel;
        if (oldVm?.Machine.Controller != null)
        {
            var oldController = oldVm.Machine.Controller;
            oldController.FrameCompleted -= OnControllerFrameCompleted;
            oldController.StateChanged -= OnControllerStateChanged;
            oldController.Machine.MachinePropertyChanged -= OnMachinePropertyChanged;
        }

        // --- Setup the events of the new controller
        var newVm = DataContext as MainWindowViewModel;
        
        if (newVm?.Machine.Controller != null)
        {
            var newController = newVm.Machine.Controller;
            newController.FrameCompleted += OnControllerFrameCompleted;
            newController.StateChanged += OnControllerStateChanged;
            newController.Machine.MachinePropertyChanged += OnMachinePropertyChanged;
            if (newController.Machine is ZxSpectrum48Machine zxMachine)
            {
                _audioProvider = new BassAudioProvider(zxMachine.BeeperDevice);
            }
            
            OnControllerStateChanged(this, (MachineControllerState.None, MachineControllerState.None));
        }
        _prevDataContext = DataContext;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = HandleKeyboardEvent(e, true);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        e.Handled = HandleKeyboardEvent(e, false);
    }

    private void OnViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        ResizeScreen();
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        if (change.Property == BoundsProperty)
        {
            ResizeScreen();
        }
        base.OnPropertyChanged(change);
    }
}

