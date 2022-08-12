using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Sets the memory range to use in the Disassembly view
/// </summary>
[CommandId("dr")]
[CommandAliases("dis-range")]
[CommandDescription("Sets the memory range to use with the disassembly view")]
[CommandUsage("dr <from address> <to address>")]
public class SetDisassemblyRangeCommand: CommandWithAddressRangeBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Output != null)
        {
            var disassembler = context.Model!.Disassembler;
            disassembler.FullRangeFrom = FromAddress;
            disassembler.FullRangeTo = ToAddress;
            disassembler.RaiseRangeChanged();
        }
        return SuccessResult;
    }
}