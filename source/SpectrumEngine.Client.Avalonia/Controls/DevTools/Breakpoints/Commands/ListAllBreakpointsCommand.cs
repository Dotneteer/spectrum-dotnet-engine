using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Adds a new breakpoint to the existing commands
/// </summary>
[CommandId("lb")]
[CommandAliases("list-bp")]
[CommandDescription("Lists breakpoints")]
[CommandUsage("lb")]
public class ListAllBreakpointsCommand: InteractiveCommandBase
{
    protected override Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
    {
        return args.Count != 0 ? Task.FromResult(ExpectNoArgs()) : SuccessMessage;
    }

    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Model != null)
        {
            var bps = context.Model.Debugger.Breakpoints;
            foreach (var bp in bps.OrderBy(b => b.Address))
            {
                WriteInfo(context, CombinedAddress(bp.Address));
            }
            WriteInfo(context, $"Displayed {bps.Count} breakpoint{(bps.Count > 1 ? "s" : "")}");
        }
        return SuccessResult;
    }
}