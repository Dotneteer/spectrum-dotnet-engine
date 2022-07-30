using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public partial class ToolsPanelUserControl : UserControl
{
    public ToolsPanelUserControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void CommandsGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TabItem tabItem && tabItem.GetLogicalChildren()
                .FirstOrDefault(li => li is CommandsPanel) is CommandsPanel commandsPanel)
        {
            await Task.Delay(50);
            commandsPanel.FocusPrompt();
        }
    }
}