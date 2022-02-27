using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpectrumEngine.Wpf.Client;

/// <summary>
/// Interaction logic for SpectrumDisplayControl.xaml
/// </summary>
public partial class SpectrumDisplayControl : UserControl
{
    private byte _currentColor;
    private WriteableBitmap _bitmap;

    public SpectrumDisplayControl()
    {
        InitializeComponent();
    }

    private DisplayViewModel? Vm => DataContext as DisplayViewModel;

    private void Instance_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppViewModel.MachineControllerState))
        {
            ResizeScreen();
        }
    }

    private void ResizeScreen()
    {
        if (DataContext is not DisplayViewModel context || context.Machine == null) return;

        var horZoom = (int)(ActualWidth / context.Machine!.ScreenWidthInPixels);
        var vertZoom = (int)(ActualHeight / context.Machine!.ScreenHeightInPixels);
        var zoom = context.ZoomFactor = Math.Max(1, Math.Min(horZoom, vertZoom));
        context.ScreenWidth = zoom * context.Machine!.ScreenWidthInPixels;
        context.ScreenHeight = zoom * context.Machine!.ScreenHeightInPixels;
    }

    private void DisplayControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ResizeScreen();
    }

    private void DisplayControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != null)
        {
            var oldVm = e.OldValue as DisplayViewModel;
            if (oldVm?.Controller != null)
            {
                oldVm.Controller.FrameCompleted -= Controller_FrameCompleted;
            }
        }
        if (e.NewValue != null)
        {
            var newVm = e.NewValue as DisplayViewModel;
            if (newVm?.Controller != null)
            {
                newVm.Controller.FrameCompleted += Controller_FrameCompleted;
            }
        }
    }

    private void Controller_FrameCompleted(object? sender, bool e)
    {
        Dispatcher.Invoke(() =>
        {
            if (Vm == null || Vm.Machine == null) return;
            _currentColor = (byte)(_currentColor + 1);
            Vm.Background = new SolidColorBrush(Color.FromArgb(0xff, _currentColor, _currentColor, _currentColor));

            var width = Vm.Machine.ScreenWidthInPixels;
            var height = Vm.Machine.ScreenHeightInPixels;

            _bitmap.Lock();
            try
            {
                unsafe
                {
                    var stride = _bitmap.BackBufferStride;
                    var buffer = Vm.Machine.GetPixelBuffer();
                    // Get a pointer to the back buffer.
                    var pBackBuffer = (long)_bitmap.BackBuffer;

                    for (var x = 0; x < width; x++)
                    {
                        for (var y = 0; y < height; y++)
                        {
                            var addr = pBackBuffer + y * stride + x * 4;
                            //uint color = (0xff000000 | (uint)(_currentColor << 16) | (uint)(_currentColor << 8) | _currentColor);
                            *(uint*)addr = buffer[y * width + x];
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

    private void DisplayControl_Loaded(object sender, RoutedEventArgs e)
    {
        ResizeScreen();
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
        Display.Width = Vm!.ScreenWidth;
        Display.Height = Vm!.ScreenHeight;
        Display.Stretch = Stretch.Fill;
    }
}
