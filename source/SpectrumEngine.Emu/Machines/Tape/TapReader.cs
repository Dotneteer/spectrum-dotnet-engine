namespace SpectrumEngine.Emu;

/// <summary>
/// This class reads a TAP file
/// </summary>
public class TapReader
{
    private readonly BinaryReader _reader;

    /// <summary>
    /// Data blocks of the TAP file
    /// </summary>
    public IList<TapeDataBlock> DataBlocks { get; }

    /// <summary>
    /// Initializes the player from the specified reader
    /// </summary>
    /// <param name="reader"></param>
    public TapReader(BinaryReader reader)
    {
        _reader = reader;
        DataBlocks = new List<TapeDataBlock>();
    }

    /// <summary>
    /// Reads in the content of the TZX file so that it can be played
    /// </summary>
    /// <returns>True, if read was successful; otherwise, false</returns>
    public virtual bool ReadContent()
    {
        try
        {
            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                var tapBlock = new TapeDataBlock();
                var length = _reader.ReadUInt16();
                tapBlock.Data = _reader.ReadBytes(length);
                DataBlocks.Add(tapBlock);
            }
            return true;
        }
        catch
        {
            // --- This exception is intentionally ignored
            return false;
        }
    }
}
