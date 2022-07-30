using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

[CommandId("help")]
[CommandAliases("?")]
[CommandDescription("Display help information about commands")]
[CommandUsage("help [command ID]")]
public class HelpCommand: InteractiveCommandBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        context.Output?.WriteLine("Help Command executing...");
        return Task.FromResult(new InteractiveCommandResult(true));
    }
}