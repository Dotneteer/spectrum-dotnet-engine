using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SpectrumEngine.Client.Avalonia.ViewModels;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class CommandsPanel : UserControl
{
    private readonly OutputBuffer _buffer = new();
    
    public CommandsPanel()
    {
        InitializeComponent();
        _buffer.ContentsChanged += (_, _) =>
        {
            if (Vm != null)
            {
                Vm.Commands.Buffer = _buffer.Clone().Contents;
            }
        };
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;
    
    private async void OnPromptKeyDown(object? _, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        _buffer.WriteLine(Prompt.Text);
        await Task.Delay(50);
        Scroller.ScrollToEnd();
        Prompt.Text = "";
    }
}