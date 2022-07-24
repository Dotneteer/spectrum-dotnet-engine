using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Tools.Commands;

public sealed class InteractiveCommandService
{
    /// <summary>
    /// Retrieves all registered commands
    /// </summary>
    public List<IInteractiveCommandInfo> GetRegisteredCommands()
    {
        return new List<IInteractiveCommandInfo>();
    }

    /// <summary>
    /// Gets the information about the command with the specified ID 
    /// </summary>
    /// <param name="id">Command identifier</param>
    /// <returns>Command information, if found; otherwise, null</returns>
    IInteractiveCommandInfo? GetCommandInfo(string id)
    {
        return null;
    }

    /**
    * Gets the information about the command with the specified ID or alias
    * @param idOrAlias
    * @returns Command information, if found; otherwise, undefined
    */
    IInteractiveCommandInfo? GetCommandByIdOrAlias(string idOrAlias)
    {
        return null;
    }

    /// <summary>
    /// Executes the specified command line 
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="buffer">Buffer that represents the output</param>
    /// <returns></returns>
    Task<InteractiveCommandResult> ExecuteCommand(string command, OutputBuffer buffer)
    {
        return Task.FromResult(new InteractiveCommandResult(true));
    }

    /// <summary>
    /// Displays the specified trace messages 
    /// </summary>
    /// <param name="messages">Trace messages to display</param>
    /// <param name="context">Context to display the messages in</param>
    public void DisplayTraceMessages(List<TraceMessage> messages, InteractiveCommandContext context
    )
    {
    }
}
