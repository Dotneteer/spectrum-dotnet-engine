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
        if (DataContext is not DisplayViewModel context || context.Machine == null) return false;
        var keyMapping = KeyMappings.DefaultMapping.FirstOrDefault(m => m.Input == e.Key);
        if (keyMapping != null)
        {
            if (context.Machine is ZxSpectrum48Machine zxMachine)
            {
                zxMachine.KeyboardDevice.SetStatus(keyMapping.Primary, isPressed);
                if (keyMapping.Secondary != null)
                {
                    zxMachine.KeyboardDevice.SetStatus(keyMapping.Secondary.Value, isPressed);
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the current view model in the data context
    /// </summary>
    private DisplayViewModel? Vm => DataContext as DisplayViewModel;
  
    /// <summary>
    /// Change the view model properties when the user control is resized.
    /// </summary>
    /// <remarks>
    /// Display the user control in the largest integer zoom factor that fits to the screen.
    /// </remarks>
    private void ResizeScreen()
    {
        if (DataContext is not DisplayViewModel context || context.Machine == null)
        {
            return;
        }
        var horZoom = (int)(Bounds.Width / context.Machine!.ScreenWidthInPixels);
        var vertZoom = (int)(Bounds.Height / context.Machine!.ScreenHeightInPixels);
        _zoomFactor = context.ZoomFactor = Math.Max(1, Math.Min(horZoom, vertZoom));
        Display.Width = context.ScreenWidth = _zoomFactor * context.Machine!.ScreenWidthInPixels;
        Display.Height = context.ScreenHeight = _zoomFactor * context.Machine!.ScreenHeightInPixels;
    }

    /// <summary>
    /// Respond to the machine controller's state changes
    /// </summary>
    private void OnControllerStateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        if (Vm?.Controller == null) return;
        
        Vm.IsDebugging = Vm.Controller.IsDebugging;
        Vm.RaisePropertyChanged(nameof(Vm.Controller));

        switch (e.NewState)
        {
            case MachineControllerState.None:
                Vm.OverlayMessage = "Start the machine";
                break;
            case MachineControllerState.Running:
                Vm.OverlayMessage = Vm.IsDebugging ? "Debug mode" : "Running";
                _audioProvider?.Play();
                break;
            case MachineControllerState.Paused:
                Vm.OverlayMessage = "Paused";
                _audioProvider?.Pause();
                break;
            case MachineControllerState.Stopped:
                Vm.OverlayMessage = "Stopped";
                _audioProvider?.Stop();
                break;
        }
    }

    private void OnControllerFrameCompleted(object? sender, bool e)
    {
        var machine = (sender as MachineController)?.Machine;
        if (machine == null) return;

        // --- Add sound samples
        var samples = (machine as ZxSpectrum48Machine)?.BeeperDevice?.GetAudioSamples();
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
        });
    }

    private void OnMachinePropertyChanged(object? sender, (string key, object? value) args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (args.key != MachinePropNames.TapeMode || args.value is not TapeMode tapeMode) return;
            if (Vm?.Controller == null) return;
            
            switch (tapeMode)
            {
                case TapeMode.Load:
                    Vm.OverlayMessage = "LOAD mode";
                    break;
                        
                case TapeMode.Save:
                    Vm.OverlayMessage = "SAVE mode";
                    break;
            }
            Vm?.RaisePropertyChanged(nameof(Vm.Controller));
        });
    }   

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // --- Release the events of the old controller
        var oldVm = _prevDataContext as DisplayViewModel;
        if (oldVm?.Controller != null)
        {
            oldVm.Controller.FrameCompleted -= OnControllerFrameCompleted;
            oldVm.Controller.StateChanged -= OnControllerStateChanged;
            oldVm.Controller.Machine.MachinePropertyChanged -= OnMachinePropertyChanged;
        }

        // --- Setup the events of the new controller
        var newVm = DataContext as DisplayViewModel;
        
        if (newVm?.Controller != null)
        {
            newVm.Controller.FrameCompleted += OnControllerFrameCompleted;
            newVm.Controller.StateChanged += OnControllerStateChanged;
            newVm.Controller.Machine.MachinePropertyChanged += OnMachinePropertyChanged;
            if (newVm.Machine is ZxSpectrum48Machine zxMachine)
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
}

