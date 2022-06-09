using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Emu;
using System;
using System.Linq;

namespace SpectrumEngine.Client.Avalonia.Controls
{
    public partial class SpectrumDisplayControl : Image
    {
        /// <summary>
        /// Defines the <see cref="MachineController"/> property.
        /// </summary>
        public static readonly DirectProperty<SpectrumDisplayControl, MachineController?> MachineControllerProperty =
            AvaloniaProperty.RegisterDirect<SpectrumDisplayControl, MachineController?>(
                nameof(MachineController),
                obj => obj.MachineController,
                (obj, value) => obj.MachineController = value);

        /// <summary>
        /// Defines the <see cref="KeyboardProvider"/> property.
        /// </summary>
        public static readonly DirectProperty<SpectrumDisplayControl, IKeyboardProvider?> KeyboardProviderProperty =
            AvaloniaProperty.RegisterDirect<SpectrumDisplayControl, IKeyboardProvider?>(
                nameof(KeyboardProvider),
                obj => obj.KeyboardProvider,
                (obj, value) => obj.KeyboardProvider = value);

        private WriteableBitmap? writeableBitmap;
        //private NAudioProvider? _audioProvider;
        private MachineController? machineController;
        private IKeyboardProvider? keyboardProvider;

        public SpectrumDisplayControl()
        {
            Initialized += SpectrumDisplayControl_Initialized;
            KeyDown += SpectrumDisplayControl_KeyDown;
            KeyUp += SpectrumDisplayControl_KeyUp;
        }

        /// <summary>
        /// Get or set ZxSpectrum Machine controller
        /// </summary>
        public MachineController? MachineController
        {
            get => machineController;
            set => SetAndRaise(MachineControllerProperty, ref machineController, value);
        }

        /// <summary>
        /// Get or set Keyboard provider
        /// </summary>
        public IKeyboardProvider? KeyboardProvider
        {
            get => keyboardProvider;
            set => SetAndRaise(KeyboardProviderProperty, ref keyboardProvider, value);
        }

        unsafe
        public override void Render(DrawingContext context)
        {
            if (machineController == null || writeableBitmap == null) return;

            CopyPixelBufferToWriteableBitmap();

            context.DrawImage(writeableBitmap,
                new Rect(0, 0, machineController.Machine.ScreenWidthInPixels, machineController.Machine.ScreenHeightInPixels),
                new Rect(0, 0, Width, Height));
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

            base.Render(context);
        }

        unsafe
        private void CopyPixelBufferToWriteableBitmap()
        {
            if (machineController == null || writeableBitmap == null) return;

            using var frameBuffer = writeableBitmap.Lock();
            uint* pFrameBuffer = (uint*)frameBuffer.Address;
            var buffer = machineController.Machine.GetPixelBuffer();

            var size = machineController.Machine.ScreenWidthInPixels * machineController.Machine.ScreenHeightInPixels;
            var offset = machineController.Machine.ScreenWidthInPixels;     // first line in Z80Machine buffer is discarted

            for (int i = 0; i < size; i++)
            {
                *pFrameBuffer = buffer[++offset];
                pFrameBuffer++;
            }
        }

        private void SpectrumDisplayControl_Initialized(object? sender, EventArgs e)
        {
            if (machineController == null) return;

            writeableBitmap = new WriteableBitmap(
                PixelSize.FromSize(new Size(machineController.Machine.ScreenWidthInPixels, machineController.Machine.ScreenHeightInPixels), 1.0),
                new Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Opaque);

            Source = writeableBitmap;
            Stretch = Stretch.Uniform;
        }

        private void SpectrumDisplayControl_KeyUp(object? sender, KeyEventArgs e) => HandleKeyboardEvent(sender, e, false);

        private void SpectrumDisplayControl_KeyDown(object? sender, KeyEventArgs e) => HandleKeyboardEvent(sender, e, true);

        /// <summary>
        /// Handle the keyboard event
        /// </summary>
        /// <remarks>
        /// This method maps a physical key to one or two ZX Spectrum keys and sets the key states through the keyboard
        /// device of the emulated machine.
        /// </remarks>
        private void HandleKeyboardEvent(object? _, KeyEventArgs e, bool isDown)
        {
            if (machineController == null || keyboardProvider == null) return;

            var keyMapping = keyboardProvider.MapKey(e.Key);
            if (keyMapping.Any() && machineController.Machine is IZxSpectrum48Machine zxMachine)
            {
                foreach (var key in keyMapping)
                {
                    zxMachine.KeyboardDevice.SetStatus(key, isDown);
                }
            }
        }

        //private void MachineController_FrameCompleted(object? sender, bool e)
        //{
        //    if (machineController == null || writeableBitmap == null) return;

        //    Dispatcher.UIThread.Post(() =>
        //    {
        //        // --- Set the next set of sound samples
        //        //var samples = (Vm.Machine as ZxSpectrum48Machine)?.BeeperDevice?.GetAudioSamples();
        //        //if (samples != null)
        //        //{
        //        //    _audioProvider?.AddSoundFrame(samples);
        //        //}

        //        // --- Display the new screen frame
        //        var width = machineController.Machine.ScreenWidthInPixels;
        //        var height = machineController.Machine.ScreenHeightInPixels;

        //        writeableBitmap.Lock();
        //        try
        //        {
        //            unsafe
        //            {
        //                var buffer = machineController.Machine.GetPixelBuffer();
        //                var pBackBuffer = writeableBitmap.BackBuffer;
        //                var offset = width;
        //                for (var y = 0; y < height; y++)
        //                {
        //                    for (var x = 0; x < width; x++)
        //                    {
        //                        *(uint*)pBackBuffer = buffer[offset];
        //                        offset++;
        //                        pBackBuffer += 4;
        //                    }
        //                }
        //            }


        //            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        //        }
        //        finally
        //        {
        //            writeableBitmap.Unlock();
        //        }
        //    });
        //}

        ///// <summary>
        ///// Respond to the machine controller's state changes
        ///// </summary>
        //private void Controller_StateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
        //{
        //    switch (e.NewState)
        //    {
        //        case MachineControllerState.Running:
        //            _audioProvider?.PlaySound();
        //            break;
        //        case MachineControllerState.Paused:
        //            _audioProvider?.PauseSound();
        //            break;
        //        case MachineControllerState.Stopped:
        //            _audioProvider?.KillSound();
        //            break;
        //    }
        //}

        private void UnregisterMachineEvents()
        {
            if (machineController != null)
            {
                //machineController.FrameCompleted -= MachineController_FrameCompleted;
                //machineController.StateChanged -= Controller_StateChanged;
            }
        }

        private void RegisterMachineEvents()
        {
            if (machineController != null)
            {
                //machineController.FrameCompleted += MachineController_FrameCompleted;
                //machineController.StateChanged += Controller_StateChanged;
                if (machineController.Machine is ZxSpectrum48Machine zxMachine)
                {
                    //_audioProvider = new(zxMachine.BeeperDevice);
                }
            }
        }
    }
}
