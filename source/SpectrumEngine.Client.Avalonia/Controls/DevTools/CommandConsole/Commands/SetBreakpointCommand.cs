using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Adds a new breakpoint to the existing commands
/// </summary>
[CommandId("sb")]
[CommandAliases("set-bp")]
[CommandDescription("Sets a breakpoint at the specified address")]
[CommandUsage("sb <address>")]
public class SetBreakpointCommand: CommandWithSingleAddressBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Model != null)
        {
            context.Model.Debugger.AddBreakpoint(Address);
            WriteInfo(context, $"Breakpoint set at {CombinedAddress(Address)}");
        }
        return SuccessResult;
    }
}