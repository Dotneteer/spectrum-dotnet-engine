using System.Collections.Generic;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Common base class for commands with no args
/// </summary>
public abstract class CommandWithNoArgBase: InteractiveCommandBase
{
    protected override Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
    {
        return args.Count != 0 ? Task.FromResult(ExpectNoArgs()) : SuccessMessage;
    }
}