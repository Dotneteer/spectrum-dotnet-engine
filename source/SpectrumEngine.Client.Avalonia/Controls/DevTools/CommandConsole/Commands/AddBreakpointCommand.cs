using System.Collections.Generic;
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
public class AddBreakpointCommand: InteractiveCommandBase
{
    /// <summary>
    /// The breakpoint address argument
    /// </summary>
    public ushort Address { get; private set; }

    protected override Task<List<TraceMessage>> ValidateArgs(List<Token> args)
    {
        return Task.FromResult(new List<TraceMessage>());
    }

    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        context.Output?.WriteLine("Add breakpoint...");
        return Task.FromResult(new InteractiveCommandResult(true));
    }
}