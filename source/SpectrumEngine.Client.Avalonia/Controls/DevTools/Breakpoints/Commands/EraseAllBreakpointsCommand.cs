using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Adds a new breakpoint to the existing commands
/// </summary>
[CommandId("eab")]
[CommandAliases("erase-bp")]
[CommandDescription("Erases all breakpoints")]
[CommandUsage("eab")]
public class EraseAllBreakpointsCommand: CommandWithNoArgBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Model != null)
        {
            var bpCount = context.Model.Debugger.Breakpoints.Count;
            context.Model.Debugger.EraseAllBreakpoints();
            WriteInfo(context, $"{bpCount} breakpoint{(bpCount > 1 ? "s": "")} removed");
        }
        return SuccessResult;
    }
}