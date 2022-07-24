using System.Reflection;

namespace SpectrumEngine.Tools.Commands;

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

    public Task<InteractiveCommandResult> Execute(InteractiveCommandContext context)
    {
        return Task.FromResult(new InteractiveCommandResult(true));
    }
    
    protected virtual Task<List<TraceMessage>> ValidateArgs(List<Token> args)
    {
        return Task.FromResult(new List<TraceMessage>());
    }

    protected abstract Task<InteractiveCommandResult> DoExecute(InteractiveCommandContext context);
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

