using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Refreshes the current disassembly view
/// </summary>
[CommandId("rsh-dis")]
[CommandDescription("Refreshes the current disassembly view")]
[CommandUsage("rsh-dis")]
public class RefreshDisassemblyCommand: CommandWithNoArgBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        context.Model?.Disassembler.RaiseRangeChanged();
        return SuccessResult;
    }
}