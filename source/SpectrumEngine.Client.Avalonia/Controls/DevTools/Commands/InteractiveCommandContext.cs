using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public interface IInteractiveCommandContext
{
    /// <summary>
    /// The command text issues
    /// </summary>
    string? CommandText { get; }
    
    /// <summary>
    /// The output buffer the command can use to write its messages
    /// </summary>
    OutputBuffer? Output { get; }
}

/// <summary>
/// This class represents the excetuin context of an interactive command
/// </summary>
public class InteractiveCommandContext : IInteractiveCommandContext
{
    /// <summary>
    /// The command text issues
    /// </summary>
    public string? CommandText { get; init; }

    /// <summary>
    /// The output buffer the command can use to write its messages
    /// </summary>
    public OutputBuffer? Output { get; init; }
}