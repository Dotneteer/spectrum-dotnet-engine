using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Refreshes the current disassembly view
/// </summary>
[CommandId("rsh-mem")]
[CommandDescription("Refreshes the current memory view")]
[CommandUsage("rsh-mem")]
public class RefreshMemoryViewCommand: CommandWithNoArgBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        context.Model?.MemoryViewer.RaiseRangeChanged();
        return SuccessResult;
    }
}