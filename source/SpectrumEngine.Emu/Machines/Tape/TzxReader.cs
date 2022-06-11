namespace SpectrumEngine.Emu;

/// <summary>
/// This class reads a TZX file
/// </summary>
public class TzxReader
{
    private readonly BinaryReader _reader;

    private static readonly Dictionary<byte, Type> s_DataBlockTypes = new()
    {
        {0x10, typeof(TzxStandardSpeedBlock)},
        {0x11, typeof(TzxTurboSpeedBlock)},
        {0x12, typeof(TzxPureToneBlock)},
        {0x13, typeof(TzxPulseSequenceBlock)},
        {0x14, typeof(TzxPureBlock)},
        {0x15, typeof(TzxDirectRecordingBlock)},
        {0x16, typeof(TzxC64RomTypeBlock)},
        {0x17, typeof(TzxC64TurboTapeBlock)},
        {0x18, typeof(TzxCswRecordingBlock)},
        {0x19, typeof(TzxGeneralizedBlock)},
        {0x20, typeof(TzxSilenceBlock)},
        {0x21, typeof(TzxGroupStartBlock)},
        {0x22, typeof(TzxGroupEndBlock)},
        {0x23, typeof(TzxJumpBlock)},
        {0x24, typeof(TzxLoopStartBlock)},
        {0x25, typeof(TzxLoopEndBlock)},
        {0x26, typeof(TzxCallSequenceBlock)},
        {0x27, typeof(TzxReturnFromSequenceBlock)},
        {0x28, typeof(TzxSelectBlock)},
        {0x2A, typeof(TzxStopTheTape48Block)},
        {0x2B, typeof(TzxSetSignalLevelBlock)},
        {0x30, typeof(TzxTextDescriptionBlock)},
        {0x31, typeof(TzxMessageBlock)},
        {0x32, typeof(TzxArchiveInfoBlock)},
        {0x33, typeof(TzxHardwareInfoBlock)},
        {0x34, typeof(TzxEmulationInfoBlock)},
        {0x35, typeof(TzxCustomInfoBlock)},
        {0x40, typeof(TzxSnapshotBlock)},
        {0x5A, typeof(TzxGlueBlock)},
    };

    /// <summary>
    /// Data blocks of this TZX file
    /// </summary>
    public IList<TzxBlockBase> DataBlocks { get; }

    /// <summary>
    /// Major version number of the file
    /// </summary>
    public byte MajorVersion { get; private set; }

    /// <summary>
    /// Minor version number of the file
    /// </summary>
    public byte MinorVersion { get; private set; }

    /// <summary>
    /// Initializes the player from the specified reader
    /// </summary>
    /// <param name="reader"></param>
    public TzxReader(BinaryReader reader)
    {
        _reader = reader;
        DataBlocks = new List<TzxBlockBase>();
    }

    /// <summary>
    /// Reads in the content of the TZX file so that it can be played
    /// </summary>
    /// <returns>True, if read was successful; otherwise, false</returns>
    public virtual bool ReadContent()
    {
        var header = new TzxHeader();
        try
        {
            header.ReadFrom(_reader);
            if (!header.IsValid)
            {
                throw new TzxException("Invalid TZX header");
            }

            MajorVersion = header.MajorVersion;
            MinorVersion = header.MinorVersion;

            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                var blockType = _reader.ReadByte();
                if (!s_DataBlockTypes.TryGetValue(blockType, out var type))
                {
                    throw new TzxException($"Unkonwn TZX block type: {blockType}");
                }

                try
                {
                    var block = Activator.CreateInstance(type) as TzxBlockBase;
                    switch (block)
                    {
                        case null:
                            throw new TzxException($"Cannot instantiate TZX data block {type}.");
                        case TzxDeprecatedBlockBase deprecated:
                            deprecated.ReadThrough(_reader);
                            break;
                        default:
                            block.ReadFrom(_reader);
                            break;
                    }

                    DataBlocks.Add(block);
                }
                catch (Exception ex)
                {
                    throw new TzxException($"Cannot read TZX data block {type}.", ex);
                }
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
