namespace SpectrumEngine.Emu;

/// <summary>
/// This interface describes the behavior of an object that
/// provides TZX tape content
/// </summary>
public interface ITapeSaver
{
    /// <summary>
    /// Creates a tape file with the specified name
    /// </summary>
    void CreateTapeFile();

    /// <summary>
    /// This method sets the name of the file according to the 
    /// Spectrum SAVE HEADER information
    /// </summary>
    /// <param name="name">Name to set</param>
    void SetName(string name);

    /// <summary>
    /// Appends the TZX block to the tape file
    /// </summary>
    /// <param name="block">Tape block to save</param>
    void SaveTapeBlock(TzxStandardSpeedBlock block);

    /// <summary>
    /// The tape provider can finalize the tape when all 
    /// TZX blocks are written.
    /// </summary>
    void FinalizeTapeFile();
}
