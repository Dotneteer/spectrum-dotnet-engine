namespace SpectrumEngine.Tools.Commands;

/// <summary>
/// The primary ID of an interactive command
/// </summary>
public class CommandIdAttribute: Attribute
{
    public string Value { get; init; }

    public CommandIdAttribute(string value)
    {
        Value = value;
    }
}

/// <summary>
/// Aliases for an interactive command
/// </summary>
public class CommandAliasesAttribute : Attribute
{
    public string[] Values { get; init; }

    public CommandAliasesAttribute(params string[] values)
    {
        Values = values;
    }
}

/// <summary>
/// The usage description of an interactive command
/// </summary>
public class CommandUsageAttribute: Attribute
{
    public string Value { get; init; }

    public CommandUsageAttribute(string value)
    {
        Value = value;
    }
}

/// <summary>
/// The help description of an interactive command
/// </summary>
public class CommandDescriptionAttribute: Attribute
{
    public string Value { get; init; }

    public CommandDescriptionAttribute(string value)
    {
        Value = value;
    }
}