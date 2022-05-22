using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class SpectrumDisplayControl : UserControl
{
    private object? _prevDataContext;
    private Rect? _effectiveViewport;
    private int _zoomFactor = 1;

    public SpectrumDisplayControl()
    {
        InitializeComponent();
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
        if (DataContext is not DisplayViewModel context || context.Machine == null || _effectiveViewport == null)
        {
            return;
        }

        var horZoom = (int)(_effectiveViewport.Value.Width / context.Machine!.ScreenWidthInPixels);
        var vertZoom = (int)(_effectiveViewport.Value.Height / context.Machine!.ScreenHeightInPixels);
        _zoomFactor = context.ZoomFactor = Math.Max(1, Math.Min(horZoom, vertZoom));
        Display.Width = context.ScreenWidth = _zoomFactor * context.Machine!.ScreenWidthInPixels;
        Display.Height = context.ScreenHeight = _zoomFactor * context.Machine!.ScreenHeightInPixels;
    }

    /// <summary>
    /// Respond to the machine controller's state changes
    /// </summary>
    private void Controller_StateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        // switch (e.NewState)
        // {
        //     case MachineControllerState.Running:
        //         _audioProvider?.PlaySound();
        //         break;
        //     case MachineControllerState.Paused:
        //         _audioProvider?.PauseSound();
        //         break;
        //     case MachineControllerState.Stopped:
        //         _audioProvider?.KillSound();
        //         break;
        // }
    }

    private void Controller_FrameCompleted(object? sender, bool e)
    {
        // --- Use the Dispatcher to render the screen
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var machine = Vm?.Machine;
            if (machine == null) return;
        
            // --- Set the next set of sound samples
            //var samples = (Vm.Machine as ZxSpectrum48Machine)?.BeeperDevice?.GetAudioSamples();
            // if (samples != null)
            // {
            //     // _audioProvider?.AddSoundFrame(samples);
            // }
        
            // --- Display the new screen frame
            var bitmap = new WriteableBitmap(
                new PixelSize(machine.ScreenWidthInPixels, machine.ScreenHeightInPixels),
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
                    for (var x = 0; x < width; x++)
                    {
                        *(uint*)pBackBuffer = buffer[offset];
                        offset++;
                        pBackBuffer += 4;
                    }
                }
            }
            Display.Source = bitmap;
        });
    }

    /// <summary>
    /// Handle the keyboard event
    /// </summary>
    /// <remarks>
    /// This method maps a physical key to one or two ZX Spectrum keys and sets the key states through the keyboard
    /// device of the emulated machine.
    /// </remarks>
    private void HandleKeyboardEvent(KeyEventArgs e, bool isPressed)
    {
        if (DataContext is not DisplayViewModel context || context.Machine == null) return;
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
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // --- Release the events of the old controller
        var oldVm = _prevDataContext as DisplayViewModel;
        if (oldVm?.Controller != null)
        {
            oldVm.Controller.FrameCompleted -= Controller_FrameCompleted;
            oldVm.Controller.StateChanged -= Controller_StateChanged;
        }

        // --- Setup the events of the new controller
        var newVm = DataContext as DisplayViewModel;
        if (newVm?.Controller != null)
        {
            newVm.Controller.FrameCompleted += Controller_FrameCompleted;
            newVm.Controller.StateChanged += Controller_StateChanged;
            if (newVm.Machine is ZxSpectrum48Machine zxMachine)
            {
                //_audioProvider = new(zxMachine.BeeperDevice);
            }
        }
        _prevDataContext = DataContext;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) => HandleKeyboardEvent(e, true);

    private void OnKeyUp(object? sender, KeyEventArgs e) => HandleKeyboardEvent(e, false);

    private void OnViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _effectiveViewport = e.EffectiveViewport;
        ResizeScreen();
    }
}

