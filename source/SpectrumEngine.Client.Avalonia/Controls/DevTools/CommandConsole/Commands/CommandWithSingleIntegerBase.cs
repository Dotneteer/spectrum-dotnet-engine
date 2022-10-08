using System.Collections.Generic;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Common base class for commands with a single address
/// </summary>
public abstract class CommandWithSingleIntegerBase: InteractiveCommandBase
{
    /// <summary>
    /// The integer argument
    /// </summary>
    protected int Arg { get; private set; }
    
    /// <summary>
    /// The minimum value of the argument (inclusive)
    /// </summary>
    protected abstract int MinValue { get;  }

    /// <summary>
    /// The maximum value of the argument (inclusive)
    /// </summary>
    protected abstract int MaxValue { get;  }

    /// <summary>
    /// Validates command arguments
    /// </summary>
    /// <param name="args">Arguments to validate</param>
    /// <returns>List of validation messages</returns>
    protected override Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
    {
        if (args.Count != 1)
        {
            return Task.FromResult(ExpectArgs(1));
        }
        var (arg, message) = GetNumericTokenValue(args[0]);
        if (arg == null)
        {
            return Task.FromResult(message!);
        }
        Arg = arg.Value;
        if (Arg < MinValue || Arg > MaxValue)
        {
            return Task.FromResult(ErrorMessage($"Argument value must be between {MinValue} and {MaxValue}"));
        }
        return SuccessMessage;
    }
}