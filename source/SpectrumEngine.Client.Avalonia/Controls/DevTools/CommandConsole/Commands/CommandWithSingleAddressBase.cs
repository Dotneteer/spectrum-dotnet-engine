using System.Collections.Generic;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Common base class fro commands with a single address
/// </summary>
public abstract class CommandWithSingleAddressBase: InteractiveCommandBase
{
    /// <summary>
    /// The address argument
    /// </summary>
    protected ushort Address { get; private set; }

    protected override Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
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
}