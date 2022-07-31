using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

[CommandId("help")]
[CommandAliases("?")]
[CommandDescription("Display help information about commands")]
[CommandUsage("help [command ID]")]
public class HelpCommand: InteractiveCommandBase
{
    private string? _commandIdArg;
    
    protected override Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
    {
        if (args.Count > 1)
        {
            return Task.FromResult(new List<ValidationMessage>
            {
                new(ValidationMessageType.Error, "This command accepts zero or one argument")
            });
        }

        _commandIdArg = args.Count == 0 ? null : args[0].Text;
        return SuccessMessage;
    }


    protected override Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context)
    {
        var output = context.Output;
        if (output == null) return SuccessResult;
        
        var commandCount = 0;
        var commands = CommandRegistry.Commands;
        foreach (var key in commands.Keys.OrderBy(k => k))
        {
            if (_commandIdArg != null && !key.ToLower().StartsWith(_commandIdArg.ToLower())) continue;
            var command = commands[key];
            output.ResetFormat();
            output.Color = OutputColors.BrightMagenta;
            output.Italic = true;
            output.Write(command.Id);
            output.Color = OutputColors.Magenta;
            output.Italic = false;
            output.Write($" [{string.Join(", ", command.Aliases)}]: ");
            output.Color = OutputColors.Green;
            output.WriteLine(command.Description);
            WriteInfo(context, $"Usage: {command.Usage}");
            commandCount++;
        }
        WriteInfo(context, $"Displayed {commandCount} command{(commandCount > 1 ? "s" : "")}");
        return SuccessResult;
    }
}