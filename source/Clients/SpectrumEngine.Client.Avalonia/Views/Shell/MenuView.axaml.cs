using Avalonia.Controls;
using Avalonia.Interactivity;
using SpectrumEngine.Emu;
using System.Linq;

namespace SpectrumEngine.Client.Avalonia.Views.Shell
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();            
        }

        private void CloseTreeItem_Tapped(object? sender, RoutedEventArgs e)
        {
            Logger.Flush();
            (this.VisualRoot as Window)?.Close();
        }
    }
}
