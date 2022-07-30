using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

    public async Task<InteractiveCommandResult> Execute(IInteractiveCommandContext context)
    {
        // --- Validate the input arguments
        var validation = await ValidateArgs(context.CommandTokens!.Skip(1).ToList());

        // --- Display validation messages and detect error
        var output = context.Output;
        var errorFound = false;
        foreach (var msg in validation)
        {
            output?.ResetFormat();
            var color = OutputColors.Cyan;
            switch (msg.Type)
            {
                case TraceMessageType.Error:
                    errorFound = true;
                    color = OutputColors.Red;
                    break;
                case TraceMessageType.Warning:
                    color = OutputColors.Yellow;
                    break;
            }

            if (output == null) continue;

            output.Color = color;
            output.WriteLine(msg.Message);
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

    protected virtual Task<List<TraceMessage>> ValidateArgs(List<Token> args)
    {
        return Task.FromResult(new List<TraceMessage>());
    }

    protected abstract Task<InteractiveCommandResult> DoExecute(IInteractiveCommandContext context);

    /// <summary>
    /// Returns a successful validation message
    /// </summary>
    public static Task<List<TraceMessage>> SuccessMessage => Task.FromResult(new List<TraceMessage>());

    /// <summary>
    /// Returns a success result
    /// </summary>
    public static Task<InteractiveCommandResult> SuccessResult => Task.FromResult(new InteractiveCommandResult(true));
    
    public static List<TraceMessage> ExpectArgs(int count)
    {
        return new List<TraceMessage>
        {
            new(TraceMessageType.Error, $"This command expects {count} argument{(count > 1 ? "s" : "")}")
        };
    }
    
    /// <summary>
    /// Converts a token to an integer value
    /// </summary>
    /// <param name="token">Token to convert</param>
    /// <returns>Integer value if conversion successful; otherwise, null</returns>
    public static (int?, List<TraceMessage>?) GetNumericTokenValue(Token token) {
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
            return (null, new List<TraceMessage>
            {
                new(TraceMessageType.Error, "Invalid numeric value")
            });
        }
    }

    /// <summary>
    /// Converts a token to a 16-bit address value
    /// </summary>
    /// <param name="token">Token to convert</param>
    /// <returns>16-bit value if conversion successful; otherwise, null</returns>
    public static (ushort?, List<TraceMessage>?) GetAddressValue(Token token)
    {
        var (value, message) = GetNumericTokenValue(token);
        return value switch
        {
            null => (null, message),
            < 0 or > ushort.MaxValue => (null,
                new List<TraceMessage> {new(TraceMessageType.Error, "Invalid 16-bit address value")}),
            _ => ((ushort) value.Value, null)
        };
    }
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
public enum TraceMessageType {
    Info,
    Warning,
    Error,
}

/// <summary>
/// Describes a trace message 
/// </summary>
/// <param name="Type">Type of the message</param>
/// <param name="Message">Message Text to display</param>
public record TraceMessage(TraceMessageType Type, string Message);

