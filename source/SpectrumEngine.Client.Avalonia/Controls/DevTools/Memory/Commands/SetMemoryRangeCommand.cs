using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Sets the memory range to use in the Memory view
/// </summary>
[CommandId("mr")]
[CommandAliases("mem-range")]
[CommandDescription("Sets the memory range to use with the memory view")]
[CommandUsage("mr <from address> <to address>")]
public class SetMemoryRangeCommand: CommandWithAddressRangeBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Output != null)
        {
            var memViewer = context.Model!.MemoryViewer;
            memViewer.RangeFrom = FromAddress;
            memViewer.RangeTo = ToAddress;
            memViewer.RaiseRangeChanged();
        }
        return SuccessResult;
    }
}