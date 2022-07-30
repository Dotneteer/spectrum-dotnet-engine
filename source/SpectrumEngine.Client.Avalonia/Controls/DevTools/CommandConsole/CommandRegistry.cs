using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

/// <summary>
/// Implements the registry that stores and executes commands
/// </summary>
public static class CommandRegistry
{
    private static readonly Dictionary<string, InteractiveCommandBase> s_Commands 
        = new(StringComparer.OrdinalIgnoreCase);

    static CommandRegistry()
    {
        s_Commands.Clear();
        RegisterCommands(Assembly.GetExecutingAssembly());
    }
    
    /// <summary>
    /// Gets the commands in the registry
    /// </summary>
    public static ReadOnlyDictionary<string, InteractiveCommandBase> Commands => new(s_Commands);

    /// <summary>
    /// Registers the commands defined in the specified assembly 
    /// </summary>
    /// <param name="asm"></param>
    private static void RegisterCommands(Assembly asm)
    {
        foreach (var type in asm.GetTypes())
        {
            if (!typeof(InteractiveCommandBase).IsAssignableFrom(type) || type.IsAbstract) continue;
            var commandInstance = (InteractiveCommandBase) Activator.CreateInstance(type)!;
            s_Commands[commandInstance.Id] = commandInstance;
        }
    }

    /// <summary>
    /// Gets a command by its ID
    /// </summary>
    /// <param name="id">ID of the command to get</param>
    /// <returns>Command instance, if found in the registry; otherwise, null</returns>
    public static InteractiveCommandBase? GetCommand(string id)
    {
        // --- Try getting a command by its ID
        if (s_Commands.TryGetValue(id, out var command))
        {
            return command;
        }
        
        // --- Try getting a command by its alias
        return s_Commands.Values.FirstOrDefault(cmd =>
            cmd.Aliases.Any(al => string.Equals(al, id, StringComparison.CurrentCultureIgnoreCase)));
    }
}