using SpectrumEngine.Emu;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpectrumEngine.Client.Wpf;

/// <summary>
/// Interaction logic for SpectrumDisplayControl.xaml
/// </summary>
public partial class SpectrumDisplayControl : UserControl
{
    private WriteableBitmap? _bitmap;
    private NAudioProvider? _audioProvider;

    /// <summary>
    /// Initialize the user control instance
    /// </summary>
    public SpectrumDisplayControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the current view model in the data context
    /// </summary>
    private DisplayViewModel? Vm => DataContext as DisplayViewModel;

    /// <summary>
    /// Initialize the display when this user control is loaded
    /// </summary>
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        var machine = Vm?.Machine;
        if (machine == null) return;

        // --- Set up the bitmap
        _bitmap = new WriteableBitmap(
                machine.ScreenWidthInPixels,
                machine.ScreenHeightInPixels,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        Display.Source = _bitmap;
        Display.Width = machine.ScreenWidthInPixels;
        Display.Height = machine.ScreenHeightInPixels;
        Display.Stretch = Stretch.Fill;
        ResizeScreen();
    }

    /// <summary>
    /// Respond to screen size changes
    /// </summary>
    private void DisplayControl_SizeChanged(object sender, SizeChangedEventArgs e) => ResizeScreen();

    /// <summary>
    /// Change the view model properties when the user control is resized.
    /// </summary>
    /// <remarks>
    /// Display the user control in the largest integer zoom factor that fits to the screen.
    /// </remarks>
    private void ResizeScreen()
    {
        if (DataContext is not DisplayViewModel context || context.Machine == null) return;

        var horZoom = (int)(ActualWidth / context.Machine!.ScreenWidthInPixels);
        var vertZoom = (int)(ActualHeight / context.Machine!.ScreenHeightInPixels);
        var zoom = context.ZoomFactor = Math.Max(1, Math.Min(horZoom, vertZoom));
        Display.Width = context.ScreenWidth = zoom * context.Machine!.ScreenWidthInPixels;
        Display.Height = context.ScreenHeight = zoom * context.Machine!.ScreenHeightInPixels;
    }

    /// <summary>
    /// Whenever the data context changes, we prepare the machine controller
    /// </summary>
    private void DisplayControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != null)
        {
            // --- Release the events of the old controller
            var oldVm = e.OldValue as DisplayViewModel;
            if (oldVm?.Controller != null)
            {
                oldVm.Controller.FrameCompleted -= Controller_FrameCompleted;
                oldVm.Controller.StateChanged -= Controller_StateChanged;
            }
        }
        if (e.NewValue != null)
        {
            // --- Setup the events of the new controller
            var newVm = e.NewValue as DisplayViewModel;
            if (newVm?.Controller != null)
            {
                newVm.Controller.FrameCompleted += Controller_FrameCompleted;
                newVm.Controller.StateChanged += Controller_StateChanged;
                if (newVm.Machine is ZxSpectrum48Machine zxMachine)
                {
                    _audioProvider = new(zxMachine.BeeperDevice);
                }
            }
        }
    }

    /// <summary>
    /// Respond to the machine controller's state changes
    /// </summary>
    private void Controller_StateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
    {
        switch (e.NewState)
        {
            case MachineControllerState.Running:
                _audioProvider?.PlaySound();
                break;
            case MachineControllerState.Paused:
                _audioProvider?.PauseSound();
                break;
            case MachineControllerState.Stopped:
                _audioProvider?.KillSound();
                break;
        }
    }

    private void Controller_FrameCompleted(object? sender, bool e)
    {
        // --- Use the Dispatcher to render the screen
        Dispatcher.Invoke(() =>
        {
            if (Vm == null || Vm.Machine == null) return;

            // --- Set the next set of sound samples
            var samples = (Vm.Machine as ZxSpectrum48Machine)?.BeeperDevice?.GetAudioSamples();
            if (samples != null)
            {
                _audioProvider?.AddSoundFrame(samples);
            }

            // --- Display the new screen frame
            var width = Vm.Machine.ScreenWidthInPixels;
            var height = Vm.Machine.ScreenHeightInPixels;

            _bitmap!.Lock();
            try
            {
                unsafe
                {
                    var buffer = Vm.Machine.GetPixelBuffer();
                    var pBackBuffer = _bitmap.BackBuffer;
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
                _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            finally
            {
                _bitmap.Unlock();
            }
        });
    }

    /// <summary>
    /// Handle the keyboard event
    /// </summary>
    /// <remarks>
    /// This method maps a physical key to one or two ZX Spectrum keys and sets the key states through the keyboard
    /// device of the emulated machine.
    /// </remarks>
    private void HandleKeyboardEvent(object sender, KeyEventArgs e)
    {
        if (DataContext is not DisplayViewModel context || context.Machine == null) return;
        var keyMapping = KeyMappings.DefaultMapping.FirstOrDefault(m => m.Input == e.Key);
        if (keyMapping != null)
        {
            if (context.Machine is ZxSpectrum48Machine zxMachine)
            {
                zxMachine.KeyboardDevice.SetStatus(keyMapping.Primary, !e.IsUp);
                if (keyMapping.Secondary != null)
                {
                    zxMachine.KeyboardDevice.SetStatus(keyMapping.Secondary.Value, !e.IsUp);
                }
            }
        }
    }
}
