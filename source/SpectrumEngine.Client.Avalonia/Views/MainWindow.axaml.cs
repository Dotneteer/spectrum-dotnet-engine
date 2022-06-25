using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private MainWindowViewModel? Vm => DataContext as MainWindowViewModel; 

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (Vm == null) return;

            var machine = new ZxSpectrum48Machine();
            machine.SetMachineProperty(MachinePropNames.TapeSaver, new DefaultTapeSaver());
            var controller = new MachineController(machine);
            Vm.Machine.SetMachineController(controller);
            Vm.Machine.CommandExecuted += (s, args) =>
            {
                FocusManager.Instance?.Focus(SpectrumDisplay);
            };
        }

        private void MainWindowOnKeyUp(object? sender, KeyEventArgs e)
        {
            e.Handled = SpectrumDisplay.HandleKeyboardEvent(e, false);
        }

        private void MainWindowOnKeyDown(object? sender, KeyEventArgs e)
        {
            e.Handled = SpectrumDisplay.HandleKeyboardEvent(e, true);
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            SplitterPanel.Height = SplitterPanel.Height > 4 ? 4 : 400;
        }

        private void SplitterPanel_OnInitialized(object? sender, EventArgs e)
        {
            
        }
    }
}