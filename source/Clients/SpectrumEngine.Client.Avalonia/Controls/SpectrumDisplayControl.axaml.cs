﻿using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Controls;

public partial class SpectrumDisplayControl : UserControl
{
    private object? _prevDataContext;
    private WriteableBitmap? _bitmap;
    private Rect? _effectiveViewport;

    public SpectrumDisplayControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the current view model in the data context
    /// </summary>
    private DisplayViewModel? Vm => DataContext as DisplayViewModel;

    // private void InitializeComponent()
    // {
    //     AvaloniaXamlLoader.Load(this);
    // }

    private void InitializeDisplay()
    {
        var machine = Vm?.Machine;
        if (machine == null) return;

        // --- Set up the bitmap
        _bitmap = new WriteableBitmap(
            new PixelSize(machine.ScreenWidthInPixels, machine.ScreenHeightInPixels),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Opaque);
        Display.Source = _bitmap;
        Display.Width = machine.ScreenWidthInPixels;
        Display.Height = machine.ScreenHeightInPixels;
        Display.Stretch = Stretch.Fill;
        ResizeScreen();
    }
    
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
        var zoom = context.ZoomFactor = Math.Max(1, Math.Min(horZoom, vertZoom));
        Display.Width = context.ScreenWidth = zoom * context.Machine!.ScreenWidthInPixels;
        Display.Height = context.ScreenHeight = zoom * context.Machine!.ScreenHeightInPixels;
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
        // // --- Use the Dispatcher to render the screen
        // Dispatcher.UIThread.InvokeAsync(() =>
        // {
        //     if (Vm?.Machine == null) return;
        //
        //     // --- Set the next set of sound samples
        //     var samples = (Vm.Machine as ZxSpectrum48Machine)?.BeeperDevice?.GetAudioSamples();
        //     if (samples != null)
        //     {
        //         // _audioProvider?.AddSoundFrame(samples);
        //     }
        //
        //     // --- Display the new screen frame
        //     var width = Vm.Machine.ScreenWidthInPixels;
        //     var height = Vm.Machine.ScreenHeightInPixels;
        //
        //     _bitmap!.Lock();
        //         // unsafe
        //         // {
        //         //     var buffer = Vm.Machine.GetPixelBuffer();
        //         //     var pBackBuffer = _bitmap.BackBuffer;
        //         //     var offset = width;
        //         //     for (var y = 0; y < height; y++)
        //         //     {
        //         //         for (var x = 0; x < width; x++)
        //         //         {
        //         //             *(uint*)pBackBuffer = buffer[offset];
        //         //             offset++;
        //         //             pBackBuffer += 4;
        //         //         }
        //         //     }
        //         // }
        //         // _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        // });
    }

    /// <summary>
    /// Handle the keyboard event
    /// </summary>
    /// <remarks>
    /// This method maps a physical key to one or two ZX Spectrum keys and sets the key states through the keyboard
    /// device of the emulated machine.
    /// </remarks>
    private void HandleKeyboardEvent(object? sender, KeyEventArgs e, bool isPressed)
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
        InitializeDisplay();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) => HandleKeyboardEvent(sender, e, true);

    private void OnKeyUp(object? sender, KeyEventArgs e) => HandleKeyboardEvent(sender, e, false);

    private void OnViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _effectiveViewport = e.EffectiveViewport;
        ResizeScreen();
    }
}

