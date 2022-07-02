namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the information about an emulated key press 
/// </summary>
public class EmulatedKeyStroke
{
    /// <summary>
    /// Start CPU tact
    /// </summary>
    public ulong StartTact { get; }

    /// <summary>
    /// End CPU tact
    /// </summary>
    public ulong EndTact { get; set; }

    /// <summary>
    /// The primary key's code
    /// </summary>
    public SpectrumKeyCode PrimaryCode { get; }

    /// <summary>
    /// Secondary key code
    /// </summary>
    public SpectrumKeyCode? SecondaryCode { get; }

    /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
    public EmulatedKeyStroke(ulong startTact, ulong endTact, SpectrumKeyCode primaryCode, 
        SpectrumKeyCode? secondaryCode)
    {
        StartTact = startTact;
        EndTact = endTact;
        PrimaryCode = primaryCode;
        SecondaryCode = secondaryCode;
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() =>
        $"S:{StartTact}, E:{EndTact}, L:{EndTact - StartTact}, {PrimaryCode}-{SecondaryCode}";
}
