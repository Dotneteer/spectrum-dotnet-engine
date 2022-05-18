using Avalonia.Controls;
using Avalonia.Interactivity;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Views.Shell
{
    public partial class ToolBarView : UserControl
    {
        public ToolBarView()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Flush();
            (this.VisualRoot as Window)?.Close();
        }
    }
}
