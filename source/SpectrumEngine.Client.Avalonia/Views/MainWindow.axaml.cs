using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SpectrumEngine.Client.Avalonia.Utility;
using SpectrumEngine.Client.Avalonia.ViewModels;
// ReSharper disable UnusedParameter.Local

namespace SpectrumEngine.Client.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            App.CloseDevToolsWindow();
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void OnClickClose(object? sender, RoutedEventArgs e)
        {
            App.CloseDevToolsWindow();
            Close();
        }
        
        private MainWindowViewModel? Vm => DataContext as MainWindowViewModel; 

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (Vm == null) return;

            const string DEFAULT_ID = "sp48"; 
            var machine = MachineFactory.CreateMachine(DEFAULT_ID);
            if (machine == null)
            {
                throw new InvalidOperationException($"Cannot find or instantiate machine with ID '{DEFAULT_ID}'");
            }
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
    }
}