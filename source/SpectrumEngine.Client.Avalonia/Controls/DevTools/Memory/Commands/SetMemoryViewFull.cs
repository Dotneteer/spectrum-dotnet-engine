using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Refreshes the current disassembly view
/// </summary>
[CommandId("mf")]
[CommandDescription("Refreshes the current memory view")]
[CommandUsage("mem-full")]
public class SetMemoryViewFull: CommandWithNoArgBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Output != null)
        {
            var memViewer = context.Model!.MemoryViewer;
            memViewer.DisplayMode = MemoryDisplayMode.Full;
            memViewer.RaiseModeChanged();
            memViewer.RaiseRangeChanged();
        }
        return SuccessResult;
    }
}