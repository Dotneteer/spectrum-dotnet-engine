using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Providers;

/// <summary>
/// The default implementation of ITapeSaver, which saves the tape data into a TZX file.
/// </summary>
public class DefaultTapeSaver: ITapeSaver
{
    /// <summary>
    /// Creates a tape file with the specified name
    /// </summary>
    public void CreateTapeFile()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// This method sets the name of the file according to the 
    /// Spectrum SAVE HEADER information
    /// </summary>
    /// <param name="name">Name to set</param>
    public void SetName(string name)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Appends the TZX block to the tape file
    /// </summary>
    /// <param name="block">Tape block to save</param>
    public void SaveTapeBlock(TzxStandardSpeedBlock block)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// The tape provider can finalize the tape when all 
    /// TZX blocks are written.
    /// </summary>
    public void FinalizeTapeFile()
    {
        throw new System.NotImplementedException();
    }
}