namespace SpectrumEngine.Emu;

/// <summary>
/// Represents a breakpoint
/// </summary>
public record BreakpointInfo
{
    /// <summary>
    /// Breakpoint address
    /// </summary>
    public ushort Address { get; set; }
    
    /// <summary>
    /// Optional partition (reserved for future use)
    /// </summary>
    public int? Partition { get; set; }

    /// <summary>
    /// Indicates an execution breakpoint
    /// </summary>
    public bool Exec { get; set; } = true;

    /// <summary>
    /// Indicates a memory read breakpoint
    /// </summary>
    public bool MemoryRead { get; set; } = false;
    
    /// <summary>
    /// Indicates a memory write breakpoint
    /// </summary>
    public bool MemoryWrite { get; set; } = false;

    /// <summary>
    /// Indicates an I/O read breakpoint
    /// </summary>
    public bool IoRead { get; set; } = false;

    /// <summary>
    /// Indicates an I/O write breakpoint
    /// </summary>
    public bool IoWrite { get; set; } = false;

    /// <summary>
    /// Optional Disassembly
    /// </summary>
    public string? Disassembly { get; set; }
}