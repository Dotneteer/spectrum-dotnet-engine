using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Adds a new breakpoint to the existing commands
/// </summary>
[CommandId("rb")]
[CommandAliases("rem-bp")]
[CommandDescription("Removes a breakpoint from the specified address")]
[CommandUsage("rb <address>")]
public class RemoveBreakpointCommand: CommandWithSingleAddressBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Model != null)
        {
            var removed = context.Model.Debugger.RemoveBreakpoint(Address);
            if (removed)
            {
                WriteInfo(context, $"Breakpoint removed from {CombinedAddress(Address)}");
            }
            else
            {
                WriteWarning(context, $"There is no breakpoint at {CombinedAddress(Address)} to remove");
            }
        }
        return SuccessResult;
    }
}