using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
            Vm?.Machine.SetMachineController(new MachineController(new ZxSpectrum48Machine()));
        }

        private void MainWindowOnKeyUp(object? sender, KeyEventArgs e)
        {
            e.Handled = SpectrumDisplay.HandleKeyboardEvent(e, false);
        }

        private void MainWindowOnKeyDown(object? sender, KeyEventArgs e)
        {
            e.Handled = SpectrumDisplay.HandleKeyboardEvent(e, true);
        }
    }
}