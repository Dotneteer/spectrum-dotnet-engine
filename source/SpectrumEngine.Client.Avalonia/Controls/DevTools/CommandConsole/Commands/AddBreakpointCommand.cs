using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;
using SpectrumEngine.Tools.Output;

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
        if (args.Count != 1)
        {
            return Task.FromResult(ExpectArgs(1));
        }
        var (address, message) = GetAddressValue(args[0]);
        if (address == null)
        {
            return Task.FromResult(message!);
        }
        Address = address.Value;
        return SuccessMessage;
    }

    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Model != null)
        {
            context.Model.Debugger.AddBreakpoint(Address);
            var output = context.Output;
            if (output != null)
            {
                output.ResetFormat();
                output.Color = OutputColors.BrightCyan;
                output.WriteLine($"Breakpoint set at ${Address:X4} ({Address}, %{Convert.ToString(Address, 2)})");
            }
        }
        return SuccessResult;
    }
}