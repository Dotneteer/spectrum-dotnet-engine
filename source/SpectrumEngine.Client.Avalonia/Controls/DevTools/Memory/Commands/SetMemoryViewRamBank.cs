using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools.Commands;

/// <summary>
/// Sets the ROM page to use in the Memory view
/// </summary>
[CommandId("mrb")]
[CommandAliases("mem-bank")]
[CommandDescription("Display the specified RAM bank in the memory view")]
[CommandUsage("mrb <RAM bank index>")]
public class SetMemoryViewRamBank: CommandWithSingleIntegerBase
{
    protected override Task<string?> ValidateCommand(IInteractiveCommandContext context) =>
        Task.FromResult(context.Model?.Machine.Id == "sp48" 
            ? "ZX Spectrum 48 does not support this command."
            : null);

    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        if (context.Model != null)
        {
            var memViewer = context.Model!.MemoryViewer;
            memViewer.DisplayMode = MemoryDisplayMode.RamBank;
            memViewer.RamBank = Arg;
            memViewer.RaiseModeChanged();
        }
        return SuccessResult;
    }

    protected override int MinValue => 0;
    protected override int MaxValue => 7;
}