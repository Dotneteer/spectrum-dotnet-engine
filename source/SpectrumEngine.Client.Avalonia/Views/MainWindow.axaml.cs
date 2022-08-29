using System;
using System.ComponentModel;
using System.Diagnostics;
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
            // AddHandler(KeyDownEvent, MainWindowOnKeyDown, RoutingStrategies.Tunnel);
            // AddHandler(KeyUpEvent, MainWindowOnKeyUp, RoutingStrategies.Tunnel);
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = SpectrumDisplay.HandleKeyboardEvent(e, true);
            if (e.Handled)
            {
                Debug.WriteLine($"Down: {e.Key}");
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            e.Handled = SpectrumDisplay.HandleKeyboardEvent(e, false);
            if (e.Handled)
            {
                Debug.WriteLine($"Up: {e.Key}");
            }
            base.OnKeyUp(e);
        }
    }
}