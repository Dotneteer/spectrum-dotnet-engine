using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Sets the memory range to use in the Memory view
/// </summary>
[CommandId("ms")]
[CommandAliases("mem-scr")]
[CommandDescription("Scroll the specified memory address into memory view")]
[CommandUsage("ms <top address>")]
public class ScrollMemoryAddressIntoViewCommand: CommandWithSingleAddressBase
{
    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        context.Model?.MemoryViewer.RaiseTopAddressChanged(Address);
        return SuccessResult;
    }
}