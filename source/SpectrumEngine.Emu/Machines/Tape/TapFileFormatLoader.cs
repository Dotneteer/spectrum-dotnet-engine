namespace SpectrumEngine.Emu;

/// <summary>
/// This class can load TAP format files
/// </summary>
public class TapFileFormatLoader
{
    /// <summary>
    /// Loads all playable data blocks from the TAP file
    /// </summary>
    /// <param name="reader">Binary reader that represents the contents of the TAP file</param>
    /// <returns>Playble data blcoks</returns>
    public List<TapeDataBlock> LoadBlocks(BinaryReader reader)
    {
        var dataBlocks = new List<TapeDataBlock>();
        // TODO: Implement read operation
        return dataBlocks;
    }
}
