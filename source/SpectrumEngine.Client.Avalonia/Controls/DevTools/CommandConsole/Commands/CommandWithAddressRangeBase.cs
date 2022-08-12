using System.Collections.Generic;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Common base class for commands with an address range
/// </summary>
public abstract class CommandWithAddressRangeBase: InteractiveCommandBase
{
    /// <summary>
    /// The "from" address argument
    /// </summary>
    protected ushort FromAddress { get; private set; }

    /// <summary>
    /// The "to" address argument
    /// </summary>
    protected ushort ToAddress { get; private set; }

    protected override Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
    {
        if (args.Count != 2)
        {
            return Task.FromResult(ExpectArgs(2));
        }
        var (addrFrom, msgFrom) = GetAddressValue(args[0], "from");
        if (addrFrom == null)
        {
            return Task.FromResult(msgFrom!);
        }
        FromAddress = addrFrom.Value;
        var (addrTo, msgTo) = GetAddressValue(args[1], "to");
        if (addrTo == null)
        {
            return Task.FromResult(msgTo!);
        }
        ToAddress = addrTo.Value;
        if (ToAddress < FromAddress)
        {
            return Task.FromResult(new List<ValidationMessage>
            {
                new(ValidationMessageType.Error, "The end of the range cannot be higher than its start")
            });
        }
        return SuccessMessage;
    }
}