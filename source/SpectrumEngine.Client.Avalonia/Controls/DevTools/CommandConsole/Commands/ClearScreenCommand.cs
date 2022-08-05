using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Clears the interactive command output
/// </summary>
[CommandId("cls")]
[CommandDescription("Clears the interactive command output")]
[CommandUsage("cls")]
public class ClearScreenCommand: CommandWithNoArgBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        context.Output?.Clear();
        return SuccessResult;
    }
}