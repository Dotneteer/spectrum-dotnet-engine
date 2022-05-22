using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Emu;

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
            Vm?.SetMachineController(new MachineController(new ZxSpectrum48Machine()));
        }
    }
}