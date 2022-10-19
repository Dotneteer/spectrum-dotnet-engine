using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SpectrumEngine.Tools.Commands;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public interface IInteractiveCommandInfo
{
    /// <summary>
    /// Primary ID of the command
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Aliases of the command
    /// </summary>
    string[] Aliases { get; }

    /// <summary>
    /// Usage information of the command
    /// </summary>
    string Usage { get; }

    /// <summary>
    /// Descirption of the command
    /// </summary>
    string Description { get; }
}

/// <summary>
/// The base class of all interactive commands
/// </summary>
public abstract class InteractiveCommandBase : IInteractiveCommandInfo
{
    /// <summary>
    /// Primary ID of the command
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Aliases of the command
    /// </summary>
    public string[] Aliases { get; }

    /// <summary>
    /// Usage information of the command
    /// </summary>
    public string Usage { get; }

    /// <summary>
    /// Descirption of the command
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initialize command attributes
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    protected InteractiveCommandBase()
    {
        var typeInfo = GetType().GetTypeInfo();
        var id = typeInfo.GetCustomAttribute<CommandIdAttribute>();
        if (id == null)
        {
            throw new InvalidOperationException("Interactive command does not have an ID");
        }

        Id = id.Value;

        var aliases = typeInfo.GetCustomAttribute<CommandAliasesAttribute>();
        Aliases = aliases?.Values ?? Array.Empty<string>();

        var usage = typeInfo.GetCustomAttribute<CommandUsageAttribute>();
        if (usage == null)
        {
            throw new InvalidOperationException("Interactive command does not have usage information");
        }

        Usage = usage.Value;

        var description = typeInfo.GetCustomAttribute<CommandDescriptionAttribute>();
        if (description == null)
        {
            throw new InvalidOperationException("Interactive command does not have help information");
        }

        Description = description.Value;
    }

    /// <summary>
    /// Executes the command in the specified context
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <returns>The result of the command</returns>
    public async Task<InteractiveCommandResult> Execute(IInteractiveCommandContext context)
    {
        // --- Validate the command
        var commandMsg = await ValidateCommand(context);
        if (commandMsg != null)
        {
            return new InteractiveCommandResult(false, commandMsg);
        }
        
        // --- Validate the input arguments
        var validation = await ValidateArgs(context.CommandTokens!.Skip(1).ToList());

        // --- Display validation messages and detect error
        var output = context.Output;
        var errorFound = false;
        foreach (var msg in validation)
        {
            output?.ResetFormat();
            switch (msg.Type)
            {
                case ValidationMessageType.Error:
                    errorFound = true;
                    WriteError(context, msg.Message);
                    break;
                case ValidationMessageType.Warning:
                    WriteWarning(context, msg.Message);
                    break;
                case ValidationMessageType.Info:
                    WriteInfo(context, msg.Message);
                    break;
            }
        }

        if (errorFound)
        {
            if (output != null)
            {
                output.ResetFormat();
                output.Color = OutputColors.Yellow;
                output.WriteLine($"Usage: {Usage}");
            }
            return new InteractiveCommandResult(false);
        }
        return await DoExecute(context);
    }

    /// <summary>
    /// Checks if the command is executable within the current context
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <returns>
    /// Null, if the command is executable; otherwise, the error message
    /// </returns>
    protected virtual Task<string?> ValidateCommand(IInteractiveCommandContext context)
    {
        return Task.FromResult((string?)null);
    }
    
    /// <summary>
    /// Validates command arguments
    /// </summary>
    /// <param name="args">Arguments to validate</param>
    /// <returns>List of validation messages</returns>
    protected virtual Task<List<ValidationMessage>> ValidateArgs(List<Token> args)
    {
        return Task.FromResult(new List<ValidationMessage>());
    }

    /// <summary>
    /// Executes the commands body. Override this method in derived command classes
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <returns>The result of the command</returns>
    protected abstract Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context);

    /// <summary>
    /// Returns a successful validation message
    /// </summary>
    public static Task<List<ValidationMessage>> SuccessMessage => Task.FromResult(new List<ValidationMessage>());

    /// <summary>
    /// Returns a success result
    /// </summary>
    public static Task<InteractiveCommandResult> SuccessResult => Task.FromResult(new InteractiveCommandResult(true));

    /// <summary>
    /// Creates a message with the specified count as the expected number of arguments.
    /// </summary>
    /// <param name="message">Error message to create</param>
    /// <returns></returns>
    public static List<ValidationMessage> ErrorMessage(string message)
    {
        return new List<ValidationMessage>
        {
            new(ValidationMessageType.Error, message)
        };
    }
    
    /// <summary>
    /// Creates a message with the specified count as the expected number of arguments.
    /// </summary>
    /// <param name="count">Number of expected arguments</param>
    /// <returns></returns>
    public static List<ValidationMessage> ExpectArgs(int count)
    {
        return new List<ValidationMessage>
        {
            new(ValidationMessageType.Error, $"This command expects {count} argument{(count > 1 ? "s" : "")}")
        };
    }
    
    /// <summary>
    /// Creates a message indicating that the command expects no arguments.
    /// </summary>
    public static List<ValidationMessage> ExpectNoArgs()
    {
        return new List<ValidationMessage>
        {
            new(ValidationMessageType.Error, "This command does not expects any arguments")
        };
    }
    
    /// <summary>
    /// Converts a token to an integer value
    /// </summary>
    /// <param name="token">Token to convert</param>
    /// <returns>Integer value if conversion successful; otherwise, null</returns>
    public static (int?, List<ValidationMessage>?) GetNumericTokenValue(Token token) {
        var plainText = token.Text.Replace("'", "").Replace("_", "");
        try
        {
            switch (token.Type)
            {
                case TokenType.DecimalLiteral:
                    return (int.Parse(plainText), null);
                case TokenType.BinaryLiteral:
                    return (Convert.ToInt32(plainText[1..], 2), null);
                case TokenType.HexadecimalLiteral:
                    return (int.Parse(plainText[1..], NumberStyles.HexNumber), null);
                default:
                    throw new Exception("Invalid token type");
            }
        }
        catch
        {
            return (null, new List<ValidationMessage>
            {
                new(ValidationMessageType.Error, "Invalid numeric value")
            });
        }
    }

    /// <summary>
    /// Converts a token to a 16-bit address value
    /// </summary>
    /// <param name="token">Token to convert</param>
    /// <param name="name">Optional argument name</param>
    /// <returns>16-bit value if conversion successful; otherwise, null</returns>
    public static (ushort?, List<ValidationMessage>?) GetAddressValue(Token token, string? name = null)
    {
        var (value, message) = GetNumericTokenValue(token);
        return value switch
        {
            null => (null, message),
            < 0 or > ushort.MaxValue => (null,
                new List<ValidationMessage> {new(ValidationMessageType.Error, 
                    $"Invalid 16-bit address value{(name == null ? "" : $" ({name})")}")}),
            _ => ((ushort) value.Value, null)
        };
    }

    /// <summary>
    /// Write an informational message to the current output
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="message">Message to write</param>
    public static void WriteInfo(IInteractiveCommandContext context, string message)
        => WriteWithColor(context, message, OutputColors.BrightCyan);

    /// <summary>
    /// Write a warning message to the current output
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="message">Message to write</param>
    public static void WriteWarning(IInteractiveCommandContext context, string message)
        => WriteWithColor(context, message, OutputColors.Yellow);

    /// <summary>
    /// Write an error message to the current output
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="message">Message to write</param>
    public static void WriteError(IInteractiveCommandContext context, string message)
        => WriteWithColor(context, message, OutputColors.Red);

    /// <summary>
    /// Write a success message to the current output
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="message">Message to write</param>
    public static void WriteSuccess(IInteractiveCommandContext context, string message)
        => WriteWithColor(context, message, OutputColors.Green);
    /// <summary>
    /// Write a message to the current output with the specified color
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="message">Message to write</param>
    /// <param name="color">Message color</param>
    public static void WriteWithColor(IInteractiveCommandContext context, string message, OutputColors color)
    {
        var output = context.Output; 
        if (output == null) return;
        output.ResetFormat();
        output.Color = color;
        output.WriteLine(message);
    }

    /// <summary>
    /// Returns an address string combining the hexadecimal, decimal, and binary representation
    /// </summary>
    /// <param name="address">Address to convert</param>
    /// <returns>Combined representation</returns>
    public static string CombinedAddress(int address)
        => $"${address:X4} ({address}, %{Convert.ToString(address, 2)})";
}

/// <summary>
/// Describes the result of a command
/// </summary>
/// <param name="Success">Indicates if the command execution was successful</param>
/// <param name="FinalMessage">Final message of the command to display</param>
public record InteractiveCommandResult(bool Success, string? FinalMessage = null);

/// <summary>
/// Available types of trace messages 
/// </summary>
public enum ValidationMessageType {
    Info,
    Warning,
    Error,
}

/// <summary>
/// Describes a trace message 
/// </summary>
/// <param name="Type">Type of the message</param>
/// <param name="Message">Message Text to display</param>
public record ValidationMessage(ValidationMessageType Type, string Message);

