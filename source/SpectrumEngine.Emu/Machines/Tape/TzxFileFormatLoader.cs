using System.Collections.ObjectModel;
using System.Text;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class can load TZX format files
/// </summary>
public class TzxFileFormatLoader
{
    /// <summary>
    /// Loads all playable data blocks from the TZX file
    /// </summary>
    /// <param name="reader">Binary reader that represents the contents of the TZX file</param>
    /// <returns>Playble data blcoks</returns>
    public List<TapeDataBlock> LoadBlocks(BinaryReader reader)
    {
        var dataBlocks = new List<TapeDataBlock>();
        // TODO: Implement read operation
        return dataBlocks;
    }
}

/// <summary>
/// This class describes a generic TZX Block
/// </summary>
public abstract class TzxBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public abstract int BlockId { get; }

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public abstract void ReadFrom(BinaryReader reader);

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public abstract void WriteTo(BinaryWriter writer);

    /// <summary>
    /// Override this method to check the content of the block
    /// </summary>
    public virtual bool IsValid => true;

    /// <summary>
    /// Returns the data block this TZX block represents
    /// </summary>
    /// <returns>Data block, if the TZX block represents one; otherwise, null</returns>
    public virtual TapeDataBlock? GetDataBlock()
        => null;
    
    /// <summary>
    /// Reads the specified number of words from the reader.
    /// </summary>
    /// <param name="reader">Reader to obtain the input from</param>
    /// <param name="count">Number of words to get</param>
    /// <returns>Word array read from the input</returns>
    public static ushort[] ReadWords(BinaryReader reader, int count)
    {
        var result = new ushort[count];
        var bytes = reader.ReadBytes(2 * count);
        for (var i = 0; i < count; i++)
        {
            result[i] = (ushort) (bytes[i * 2] + bytes[i * 2 + 1] << 8);
        }

        return result;
    }

    /// <summary>
    /// Writes the specified array of words to the writer
    /// </summary>
    /// <param name="writer">Output</param>
    /// <param name="words">Word array</param>
    public static void WriteWords(BinaryWriter writer, ushort[] words)
    {
        foreach (var word in words)
        {
            writer.Write(word);
        }
    }

    /// <summary>
    /// Converts the provided bytes to an ASCII string
    /// </summary>
    /// <param name="bytes">Bytes to convert</param>
    /// <param name="offset">First byte offset</param>
    /// <param name="count">Number of bytes</param>
    /// <returns>ASCII string representation</returns>
    public static string ToAsciiString(byte[] bytes, int offset = 0, int count = -1)
    {
        if (count < 0) count = bytes.Length - offset;
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            sb.Append(Convert.ToChar(bytes[i + offset]));
        }

        return sb.ToString();
    }
}

/// <summary>
/// Base class for all TZX block type with data length of 3 bytes
/// </summary>
public abstract class Tzx3ByteBlockBase : TzxBlockBase
{
    /// <summary>
    /// Used bits in the last byte (other bits should be 0)
    /// </summary>
    /// <remarks>
    /// (e.g. if this is 6, then the bits used(x) in the last byte are: 
    /// xxxxxx00, where MSb is the leftmost bit, LSb is the rightmost bit)
    /// </remarks>
    public byte LastByteUsedBits { get; set; }

    /// <summary>
    /// Lenght of block data
    /// </summary>
    public byte[]? DataLength { get; set; }

    /// <summary>
    /// Block Data
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// Override this method to check the content of the block
    /// </summary>
    public override bool IsValid => GetLength() == Data?.Length;

    /// <summary>
    /// Calculates data length
    /// </summary>
    protected int GetLength()
    {
        return DataLength![0] + (DataLength[1] << 8) + (DataLength[2] << 16);
    }
}

/// <summary>
/// This class represents a TZX data block with empty body
/// </summary>
public abstract class TzxBodylessBlockBase : TzxBlockBase
{
    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
    }
}

/// <summary>
/// This class represents a deprecated TZX block
/// </summary>
public abstract class TzxDeprecatedBlockBase : TzxBlockBase
{
    /// <summary>
    /// Reads through the block infromation, and does not store it
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public abstract void ReadThrough(BinaryReader reader);

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        throw new InvalidOperationException("Deprecated TZX data blocks cannot be written.");
    }
}

/// <summary>
/// Represents the archive info block in a TZX file
/// </summary>
public class TzxArchiveInfoBlock : Tzx3ByteBlockBase
{
    /// <summary>
    /// Length of the whole block (without these two bytes)
    /// </summary>
    public ushort Length { get; set; }

    /// <summary>
    /// Number of text strings
    /// </summary>
    public byte StringCount { get; set; }

    /// <summary>
    /// List of text strings
    /// </summary>
    public TzxText[]? TextStrings { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x32;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Length = reader.ReadUInt16();
        StringCount = reader.ReadByte();
        TextStrings = new TzxText[StringCount];
        for (var i = 0; i < StringCount; i++)
        {
            var text = new TzxText();
            text.ReadFrom(reader);
            TextStrings[i] = text;
        }
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Length);
        writer.Write(StringCount);
        if (TextStrings == null) return;
        foreach (var text in TextStrings)
        {
            text.WriteTo(writer);
        }
    }
}

/// <summary>
/// This block was created to support the Commodore 64 standard 
/// ROM and similar tape blocks.
/// </summary>
public class TzxC64RomTypeBlock : TzxDeprecatedBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x16;

    /// <summary>
    /// Reads through the block infromation, and does not store it
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadThrough(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        reader.ReadBytes(length - 4);
    }
}

/// <summary>
/// This block is made to support another type of encoding that is 
/// commonly used by the C64.
/// </summary>
public class TzxC64TurboTapeBlock : TzxDeprecatedBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x17;

    /// <summary>
    /// Reads through the block infromation, and does not store it
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadThrough(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        reader.ReadBytes(length - 4);
    }
}

/// <summary>
/// This block is an analogue of the CALL Subroutine statement.
/// </summary>
/// <remarks>
/// It basically executes a sequence of blocks that are somewhere else and then goes back to the next block. Because
/// more than one call can be normally used you can include a list of sequences to be called. The 'nesting' of call
/// blocks is also not allowed for the simplicity reasons. You can, of course, use the CALL blocks in the LOOP sequences
/// and vice versa.
/// </remarks>
public class TzxCallSequenceBlock : TzxBlockBase
{
    /// <summary>
    /// Number of group name
    /// </summary>
    public byte NumberOfCalls { get; set; }

    /// <summary>
    /// Group name bytes
    /// </summary>
    public ushort[]? BlockOffsets { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x26;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        NumberOfCalls = reader.ReadByte();
        BlockOffsets = ReadWords(reader, NumberOfCalls);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(NumberOfCalls);
        if (BlockOffsets == null) return;
        WriteWords(writer, BlockOffsets);
    }
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxCustomInfoBlock : Tzx3ByteBlockBase
{
    /// <summary>
    /// Identification string (in ASCII)
    /// </summary>
    public byte[]? Id { get; set; }

    /// <summary>
    /// String representation of the ID
    /// </summary>
    public string IdText => ToAsciiString(Id!);

    /// <summary>
    /// Length of the custom info
    /// </summary>
    public uint Length { get; set; }

    /// <summary>
    /// Custom information
    /// </summary>
    public byte[]? CustomInfo { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x35;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Id = reader.ReadBytes(10);
        Length = reader.ReadUInt32();
        CustomInfo = reader.ReadBytes((int)Length);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Id!);
        writer.Write(Length);
        writer.Write(CustomInfo!);
    }
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxCswRecordingBlock : TzxBlockBase
{
    /// <summary>
    /// Block length (without these four bytes)
    /// </summary>
    public uint BlockLength { get; set; }

    /// <summary>
    /// Pause after this block
    /// </summary>
    public ushort PauseAfter { get; set; }

    /// <summary>
    /// Sampling rate
    /// </summary>
    public byte[]? SamplingRate { get; set; }

    /// <summary>
    /// Compression type
    /// </summary>
    /// <remarks>
    /// 0x01=RLE, 0x02=Z-RLE
    /// </remarks>
    public byte CompressionType { get; set; }

    /// <summary>
    /// Number of stored pulses (after decompression, for validation purposes)
    /// </summary>
    public uint PulseCount { get; set; }

    /// <summary>
    /// CSW data, encoded according to the CSW file format specification
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x18;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        BlockLength = reader.ReadUInt32();
        PauseAfter = reader.ReadUInt16();
        SamplingRate = reader.ReadBytes(3);
        CompressionType = reader.ReadByte();
        PulseCount = reader.ReadUInt32();
        var length = (int) BlockLength - 4 /* PauseAfter*/ - 3 /* SamplingRate */
                     - 1 /* CompressionType */ - 4 /* PulseCount */;
        Data = reader.ReadBytes(length);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(BlockLength);
        writer.Write(PauseAfter);
        writer.Write(SamplingRate!);
        writer.Write(CompressionType);
        writer.Write(PulseCount);
        writer.Write(Data!);
    }

    /// <summary>
    /// Override this method to check the content of the block
    /// </summary>
    public override bool IsValid => BlockLength == 4 + 3 + 1 + 4 + Data!.Length;
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxDirectRecordingBlock : Tzx3ByteBlockBase
{
    /// <summary>
    /// Number of T-states per sample (bit of data)
    /// </summary>
    public ushort TactsPerSample { get; set; }

    /// <summary>
    /// Pause after this block
    /// </summary>
    public ushort PauseAfter { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x15;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        TactsPerSample = reader.ReadUInt16();
        PauseAfter = reader.ReadUInt16();
        LastByteUsedBits = reader.ReadByte();
        DataLength = reader.ReadBytes(3);
        Data = reader.ReadBytes(GetLength());
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(TactsPerSample);
        writer.Write(PauseAfter);
        writer.Write(LastByteUsedBits);
        writer.Write(DataLength!);
        writer.Write(Data!);
    }
}

/// <summary>
/// This is a special block that would normally be generated only by emulators.
/// </summary>
public class TzxEmulationInfoBlock : TzxDeprecatedBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x34;

    /// <summary>
    /// Reads through the block infromation, and does not store it
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadThrough(BinaryReader reader)
    {
        reader.ReadBytes(8);
    }
}

/// <summary>
/// Represents a generalized data block in a TZX file
/// </summary>
public class TzxGeneralizedBlock : TzxBlockBase
{
    /// <summary>
    /// Block length (without these four bytes)
    /// </summary>
    public uint BlockLength { get; set; }

    /// <summary>
    /// Pause after this block 
    /// </summary>
    public ushort PauseAfter { get; set; }

    /// <summary>
    /// Total number of symbols in pilot/sync block (can be 0)
    /// </summary>
    public uint Totp { get; set; }

    /// <summary>
    /// Maximum number of pulses per pilot/sync symbol
    /// </summary>
    public byte Npp { get; set; }

    /// <summary>
    /// Number of pilot/sync symbols in the alphabet table (0=256)
    /// </summary>
    public byte Asp { get; set; }

    /// <summary>
    /// Total number of symbols in data stream (can be 0)
    /// </summary>
    public uint Totd { get; set; }

    /// <summary>
    /// Maximum number of pulses per data symbol
    /// </summary>
    public byte Npd { get; set; }

    /// <summary>
    /// Number of data symbols in the alphabet table (0=256)
    /// </summary>
    public byte Asd { get; set; }

    /// <summary>
    /// Pilot and sync symbols definition table
    /// </summary>
    /// <remarks>
    /// This field is present only if Totp > 0
    /// </remarks>
    public TzxSymDef[]? PilotSymDef { get; set; }

    /// <summary>
    /// Pilot and sync data stream
    /// </summary>
    /// <remarks>
    /// This field is present only if Totd > 0
    /// </remarks>
    public TzxPrle[]? PilotStream { get; set; }

    /// <summary>
    /// Data symbols definition table
    /// </summary>
    /// <remarks>
    /// This field is present only if Totp > 0
    /// </remarks>
    public TzxSymDef[]? DataSymDef { get; set; }

    /// <summary>
    /// Data stream
    /// </summary>
    /// <remarks>
    /// This field is present only if Totd > 0
    /// </remarks>
    public TzxPrle[]? DataStream { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x19;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        BlockLength = reader.ReadUInt32();
        PauseAfter = reader.ReadUInt16();
        Totp = reader.ReadUInt32();
        Npp = reader.ReadByte();
        Asp = reader.ReadByte();
        Totd = reader.ReadUInt32();
        Npd = reader.ReadByte();
        Asd = reader.ReadByte();

        PilotSymDef = new TzxSymDef[Asp];
        for (var i = 0; i < Asp; i++)
        {
            var symDef = new TzxSymDef(Npp);
            symDef.ReadFrom(reader);
            PilotSymDef[i] = symDef;
        }

        PilotStream = new TzxPrle[Totp];
        for (var i = 0; i < Totp; i++)
        {
            PilotStream[i].Symbol = reader.ReadByte();
            PilotStream[i].Repetitions = reader.ReadUInt16();
        }

        DataSymDef = new TzxSymDef[Asd];
        for (var i = 0; i < Asd; i++)
        {
            var symDef = new TzxSymDef(Npd);
            symDef.ReadFrom(reader);
            DataSymDef[i] = symDef;
        }

        DataStream = new TzxPrle[Totd];
        for (var i = 0; i < Totd; i++)
        {
            DataStream[i].Symbol = reader.ReadByte();
            DataStream[i].Repetitions = reader.ReadUInt16();
        }
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(BlockLength);
        writer.Write(PauseAfter);
        writer.Write(Totp);
        writer.Write(Npp);
        writer.Write(Asp);
        writer.Write(Totd);
        writer.Write(Npd);
        writer.Write(Asd);
        for (var i = 0; i < Asp; i++)
        {
            PilotSymDef![i].WriteTo(writer);
        }

        for (var i = 0; i < Totp; i++)
        {
            writer.Write(PilotStream![i].Symbol);
            writer.Write(PilotStream[i].Repetitions);
        }

        for (var i = 0; i < Asd; i++)
        {
            DataSymDef![i].WriteTo(writer);
        }

        for (var i = 0; i < Totd; i++)
        {
            writer.Write(DataStream![i].Symbol);
            writer.Write(DataStream[i].Repetitions);
        }
    }
}

/// <summary>
/// This block is generated when you merge two ZX Tape files together.
/// </summary>
/// <remarks>
/// It is here so that you can easily copy the files together and use 
/// them. Of course, this means that resulting file would be 10 bytes 
/// longer than if this block was not used. All you have to do if 
/// you encounter this block ID is to skip next 9 bytes.
/// </remarks>
public class TzxGlueBlock : TzxBlockBase
{
    /// <summary>
    /// Value: { "XTape!", 0x1A, MajorVersion, MinorVersion }
    /// </summary>
    /// <remarks>
    /// Just skip these 9 bytes and you will end up on the next ID.
    /// </remarks>
    public byte[]? Glue { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x5A;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Glue = reader.ReadBytes(9);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Glue!);
    }
}

/// <summary>
/// This indicates the end of a group. This block has no body.
/// </summary>
public class TzxGroupEndBlock : TzxBodylessBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x22;
}

/// <summary>
/// This block marks the start of a group of blocks which are 
/// to be treated as one single (composite) block.
/// </summary>
public class TzxGroupStartBlock : TzxBlockBase
{
    /// <summary>
    /// Number of group name
    /// </summary>
    public byte Length { get; set; }

    /// <summary>
    /// Group name bytes
    /// </summary>
    public byte[]? Chars { get; set; }

    /// <summary>
    /// Gets the group name
    /// </summary>
    public string GroupName => ToAsciiString(Chars!);

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x21;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Length = reader.ReadByte();
        Chars = reader.ReadBytes(Length);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Length);
        writer.Write(Chars!);
    }
}

/// <summary>
/// 
/// </summary>
public class TzxHardwareInfoBlock : TzxBlockBase
{
    /// <summary>
    /// Number of machines and hardware types for which info is supplied
    /// </summary>
    public byte HwCount { get; set; }

    /// <summary>
    /// List of machines and hardware
    /// </summary>
    public TzxHwInfo[]? HwInfo { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x33;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        HwCount = reader.ReadByte();
        HwInfo = new TzxHwInfo[HwCount];
        for (var i = 0; i < HwCount; i++)
        {
            var hw = new TzxHwInfo();
            hw.ReadFrom(reader);
            HwInfo[i] = hw;
        }
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(HwCount);
        if (HwInfo == null) return;
        foreach (var hw in HwInfo)
        {
            hw.WriteTo(writer);
        }
    }
}

/// <summary>
/// Represents the header of the TZX file
/// </summary>
public class TzxHeader : TzxBlockBase
{
    public static IReadOnlyList<byte> TzxSignature =
        new ReadOnlyCollection<byte>(new byte[] {0x5A, 0x58, 0x54, 0x61, 0x70, 0x65, 0x21});

    public byte[] Signature { get; private set; }
    public byte Eot { get; private set; }
    public byte MajorVersion { get; private set; }
    public byte MinorVersion { get; private set; }

    public TzxHeader(byte majorVersion = 1, byte minorVersion = 20)
    {
        Signature = TzxSignature.ToArray();
        Eot = 0x1A;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x00;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Signature = reader.ReadBytes(7);
        Eot = reader.ReadByte();
        MajorVersion = reader.ReadByte();
        MinorVersion = reader.ReadByte();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Signature);
        writer.Write(Eot);
        writer.Write(MajorVersion);
        writer.Write(MinorVersion);
    }

    #region Overrides of TzxDataBlockBase

    /// <summary>
    /// Override this method to check the content of the block
    /// </summary>
    public override bool IsValid => Signature.SequenceEqual(TzxSignature)
                                    && Eot == 0x1A
                                    && MajorVersion == 1;

    #endregion
}

/// <summary>
/// This block will enable you to jump from one block to another within the file.
/// </summary>
/// <remarks>
/// Jump 0 = 'Loop Forever' - this should never happen
/// Jump 1 = 'Go to the next block' - it is like NOP in assembler
/// Jump 2 = 'Skip one block'
/// Jump -1 = 'Go to the previous block'
/// </remarks>
public class TzxJumpBlock : TzxBlockBase
{
    /// <summary>
    /// Relative jump value
    /// </summary>
    /// <remarks>
    /// </remarks>
    public short Jump { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x23;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Jump = reader.ReadInt16();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Jump);
    }
}

/// <summary>
/// It means that the utility should jump back to the start 
/// of the loop if it hasn't been run for the specified number 
/// of times.
/// </summary>
public class TzxLoopEndBlock : TzxBodylessBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x25;
}

/// <summary>
/// If you have a sequence of identical blocks, or of identical 
/// groups of blocks, you can use this block to tell how many 
/// times they should be repeated.
/// </summary>
public class TzxLoopStartBlock : TzxBlockBase
{
    /// <summary>
    /// Number of repetitions (greater than 1)
    /// </summary>
    public ushort Loops { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x24;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Loops = reader.ReadUInt16();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Loops);
    }
}

/// <summary>
/// This will enable the emulators to display a message for a given time.
/// </summary>
/// <remarks>
/// This should not stop the tape and it should not make silence. If the 
/// time is 0 then the emulator should wait for the user to press a key.
/// </remarks>
public class TzxMessageBlock: TzxBlockBase
{
    /// <summary>
    /// Time (in seconds) for which the message should be displayed
    /// </summary>
    public byte Time { get; set; }

    /// <summary>
    /// Length of the description
    /// </summary>
    public byte MessageLength { get; set; }

    /// <summary>
    /// The description bytes
    /// </summary>
    public byte[]? Message;

    /// <summary>
    /// The string form of description
    /// </summary>
    public string MessageText => ToAsciiString(Message!);

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x31;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Time = reader.ReadByte();
        MessageLength = reader.ReadByte();
        Message = reader.ReadBytes(MessageLength);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(MessageLength);
        writer.Write(Message!);
    }
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxPulseSequenceBlock : TzxBlockBase
{
    /// <summary>
    /// Pause after this block
    /// </summary>
    public byte PulseCount { get; set; }

    /// <summary>
    /// Lenght of block data
    /// </summary>
    public ushort[]? PulseLengths { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x13;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        PulseCount = reader.ReadByte();
        PulseLengths = ReadWords(reader, PulseCount);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(PulseCount);
        WriteWords(writer, PulseLengths!);
    }

    /// <summary>
    /// Override this method to check the content of the block
    /// </summary>
    public override bool IsValid => PulseCount == PulseLengths!.Length;
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxPureBlock : Tzx3ByteBlockBase
{
    /// <summary>
    /// Length of the zero bit
    /// </summary>
    public ushort ZeroBitPulseLength { get; set; }

    /// <summary>
    /// Length of the one bit
    /// </summary>
    public ushort OneBitPulseLength { get; set; }

    /// <summary>
    /// Pause after this block
    /// </summary>
    public ushort PauseAfter { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x14;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        ZeroBitPulseLength = reader.ReadUInt16();
        OneBitPulseLength = reader.ReadUInt16();
        LastByteUsedBits = reader.ReadByte();
        PauseAfter = reader.ReadUInt16();
        DataLength = reader.ReadBytes(3);
        Data = reader.ReadBytes(GetLength());
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(ZeroBitPulseLength);
        writer.Write(OneBitPulseLength);
        writer.Write(LastByteUsedBits);
        writer.Write(PauseAfter);
        writer.Write(DataLength!);
        writer.Write(Data!);
    }
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxPureToneBlock : TzxBlockBase
{
    /// <summary>
    /// Pause after this block
    /// </summary>
    public ushort PulseLength { get; private set; }

    /// <summary>
    /// Lenght of block data
    /// </summary>
    public ushort PulseCount { get; private set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x12;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        PulseLength = reader.ReadUInt16();
        PulseCount = reader.ReadUInt16();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(PulseLength);
        writer.Write(PulseCount);
    }
}

/// <summary>
/// This block indicates the end of the Called Sequence.
/// The next block played will be the block after the last 
/// CALL block
/// </summary>
public class TzxReturnFromSequenceBlock : TzxBodylessBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x27;
}

/// <summary>
/// Pause (silence) or 'Stop the Tape' block
/// </summary>
public class TzxSelectBlock : TzxBlockBase
{
    /// <summary>
    /// Length of the whole block (without these two bytes)
    /// </summary>
    public ushort Length { get; set; }

    /// <summary>
    /// Number of selections
    /// </summary>
    public byte SelectionCount { get; set; }

    /// <summary>
    /// List of selections
    /// </summary>
    public TzxSelect[]? Selections { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x28;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Length = reader.ReadUInt16();
        SelectionCount = reader.ReadByte();
        Selections = new TzxSelect[SelectionCount];
        foreach (var selection in Selections)
        {
            selection.ReadFrom(reader);
        }
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Length);
        writer.Write(SelectionCount);
        if (Selections == null) return;
        foreach (var selection in Selections)
        {
            selection.WriteTo(writer);
        }
    }
}

/// <summary>
/// This block sets the current signal level to the specified value (high or low).
/// </summary>
public class TzxSetSignalLevelBlock : TzxBlockBase
{
    /// <summary>
    /// Length of the block without these four bytes
    /// </summary>
    public uint Lenght { get; } = 1;

    /// <summary>
    /// Signal level (0=low, 1=high)
    /// </summary>
    public byte SignalLevel { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x2B;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        reader.ReadUInt32();
        SignalLevel = reader.ReadByte();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Lenght);
        writer.Write(SignalLevel);
    }
}

/// <summary>
/// Pause (silence) or 'Stop the Tape' block
/// </summary>
public class TzxSilenceBlock : TzxBlockBase
{
    /// <summary>
    /// Duration of silence
    /// </summary>
    /// <remarks>
    /// This will make a silence (low amplitude level (0)) for a given time 
    /// in milliseconds. If the value is 0 then the emulator or utility should 
    /// (in effect) STOP THE TAPE, i.e. should not continue loading until 
    /// the user or emulator requests it.
    /// </remarks>
    public ushort Duration { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x20;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        Duration = reader.ReadUInt16();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Duration);
    }
}

/// <summary>
/// This block was created to support the Commodore 64 standard 
/// ROM and similar tape blocks.
/// </summary>
public class TzxSnapshotBlock : TzxDeprecatedBlockBase
{
    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x40;

    /// <summary>
    /// Reads through the block infromation, and does not store it
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadThrough(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        length = length & 0x00FFFFFF;
        reader.ReadBytes(length);
    }
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxStandardSpeedBlock : TzxBlockBase
{
    /// <summary>
    /// Pause after this block (default: 1000ms)
    /// </summary>
    public ushort PauseAfter { get; set; } = 1000;

    /// <summary>
    /// Lenght of block data
    /// </summary>
    public ushort DataLength { get; set; }

    /// <summary>
    /// Block Data
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x10;
    
    /// <summary>
    /// Returns the data block this TZX block represents
    /// </summary>
    /// <returns>Data block, if the TZX block represents one; otherwise, null</returns>
    public override TapeDataBlock GetDataBlock()
        => new TapeDataBlock { Data = Data! };

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        PauseAfter = reader.ReadUInt16();
        DataLength = reader.ReadUInt16();
        Data = reader.ReadBytes(DataLength);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte) BlockId);
        writer.Write(PauseAfter);
        writer.Write(DataLength);
        writer.Write(Data!, 0, DataLength);
    }
}

/// <summary>
/// When this block is encountered, the tape will stop ONLY if 
/// the machine is an 48K Spectrum.
/// </summary>
/// <remarks>
/// This block is to be used for multiloading games that load one 
/// level at a time in 48K mode, but load the entire tape at once 
/// if in 128K mode. This block has no body of its own, but follows 
/// the extension rule.
/// </remarks>
public class TzxStopTheTape48Block : TzxBlockBase
{
    /// <summary>
    /// Length of the block without these four bytes (0)
    /// </summary>
    public uint Lenght { get; } = 0;

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x2A;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        reader.ReadUInt32();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(Lenght);
    }
}

/// <summary>
/// This is meant to identify parts of the tape, so you know where level 1 starts, 
/// where to rewind to when the game ends, etc.
/// </summary>
/// <remarks>
/// This description is not guaranteed to be shown while the tape is playing, 
/// but can be read while browsing the tape or changing the tape pointer.
/// </remarks>
public class TzxTextDescriptionBlock: TzxBlockBase
{
    /// <summary>
    /// Length of the description
    /// </summary>
    public byte DescriptionLength { get; set; }

    /// <summary>
    /// The description bytes
    /// </summary>
    public byte[]? Description;

    /// <summary>
    /// The string form of description
    /// </summary>
    public string DescriptionText => ToAsciiString(Description!);

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x30;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        DescriptionLength = reader.ReadByte();
        Description = reader.ReadBytes(DescriptionLength);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(DescriptionLength);
        writer.Write(Description!);
    }
}

/// <summary>
/// Represents the standard speed data block in a TZX file
/// </summary>
public class TzxTurboSpeedBlock : Tzx3ByteBlockBase
{
    /// <summary>
    /// Length of pilot pulse
    /// </summary>
    public ushort PilotPulseLength { get; set; }

    /// <summary>
    /// Length of the first sync pulse
    /// </summary>
    public ushort Sync1PulseLength { get; set; }

    /// <summary>
    /// Length of the second sync pulse
    /// </summary>
    public ushort Sync2PulseLength { get; set; }

    /// <summary>
    /// Length of the zero bit
    /// </summary>
    public ushort ZeroBitPulseLength { get; set; }

    /// <summary>
    /// Length of the one bit
    /// </summary>
    public ushort OneBitPulseLength { get; set; }

    /// <summary>
    /// Length of the pilot tone
    /// </summary>
    public ushort PilotToneLength { get; set; }

    /// <summary>
    /// Pause after this block
    /// </summary>
    public ushort PauseAfter { get; set; }

    public TzxTurboSpeedBlock()
    {
        PilotPulseLength = 2168;
        Sync1PulseLength = 667;
        Sync2PulseLength = 735;
        ZeroBitPulseLength = 855;
        OneBitPulseLength = 1710;
        PilotToneLength = 8063;
        LastByteUsedBits = 8;
    }

    /// <summary>
    /// The ID of the block
    /// </summary>
    public override int BlockId => 0x11;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public override void ReadFrom(BinaryReader reader)
    {
        PilotPulseLength = reader.ReadUInt16();
        Sync1PulseLength = reader.ReadUInt16();
        Sync2PulseLength = reader.ReadUInt16();
        ZeroBitPulseLength = reader.ReadUInt16();
        OneBitPulseLength = reader.ReadUInt16();
        PilotToneLength = reader.ReadUInt16();
        LastByteUsedBits = reader.ReadByte();
        PauseAfter = reader.ReadUInt16();
        DataLength = reader.ReadBytes(3);
        Data = reader.ReadBytes(GetLength());
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public override void WriteTo(BinaryWriter writer)
    {
        writer.Write(PilotPulseLength);
        writer.Write(Sync1PulseLength);
        writer.Write(Sync2PulseLength);
        writer.Write(ZeroBitPulseLength);
        writer.Write(OneBitPulseLength);
        writer.Write(PilotToneLength);
        writer.Write(LastByteUsedBits);
        writer.Write(PauseAfter);
        writer.Write(DataLength!);
        writer.Write(Data!);
    }

}

/// <summary>
/// This blocks contains information about the hardware that the programs on this tape use.
/// </summary>
public class TzxHwInfo
{
    /// <summary>
    /// Hardware type
    /// </summary>
    public byte HwType { get; set; }

    /// <summary>
    /// Hardwer Id
    /// </summary>
    public byte HwId { get; set; }

    /// <summary>
    /// Information about the tape
    /// </summary>
    /// <remarks>
    /// 00 - The tape RUNS on this machine or with this hardware,
    ///      but may or may not use the hardware or special features of the machine.
    /// 01 - The tape USES the hardware or special features of the machine,
    ///      such as extra memory or a sound chip.
    /// 02 - The tape RUNS but it DOESN'T use the hardware
    ///      or special features of the machine.
    /// 03 - The tape DOESN'T RUN on this machine or with this hardware.
    /// </remarks>
    public byte TapeInfo;

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public void ReadFrom(BinaryReader reader)
    {
        HwType = reader.ReadByte();
        HwId = reader.ReadByte();
        TapeInfo = reader.ReadByte();
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(HwType);
        writer.Write(HwId);
        writer.Write(TapeInfo);
    }
}

/// <summary>
/// This is meant to identify parts of the tape, so you know where level 1 starts, where to rewind to when the game
/// ends, etc.
/// </summary>
/// <remarks>
/// This description is not guaranteed to be shown while the tape is playing, but can be read while browsing the tape or
/// changing the tape pointer.
/// </remarks>
public class TzxText
{
    /// <summary>
    /// Text identification byte.
    /// </summary>
    /// <remarks>
    /// 00 - Full title
    /// 01 - Software house/publisher
    /// 02 - Author(s)
    /// 03 - Year of publication
    /// 04 - Language
    /// 05 - Game/utility type
    /// 06 - Price
    /// 07 - Protection scheme/loader
    /// 08 - Origin
    /// FF - Comment(s)
    /// </remarks>
    public byte Type { get; set; }

    /// <summary>
    /// Length of the description
    /// </summary>
    public byte Length { get; set; }

    /// <summary>
    /// The description bytes
    /// </summary>
    public byte[]? TextBytes;

    /// <summary>
    /// The string form of description
    /// </summary>
    public string Text => TzxBlockBase.ToAsciiString(TextBytes!);

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public void ReadFrom(BinaryReader reader)
    {
        Type = reader.ReadByte();
        Length = reader.ReadByte();
        TextBytes = reader.ReadBytes(Length);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Type);
        writer.Write(Length);
        writer.Write(TextBytes!);
    }
}

/// <summary>
/// This block represents an extremely wide range of data encoding techniques.
/// </summary>
/// <remarks>
/// The basic idea is that each loading component (pilot tone, sync pulses, data) 
/// is associated to a specific sequence of pulses, where each sequence (wave) can 
/// contain a different number of pulses from the others. In this way we can have 
/// a situation where bit 0 is represented with 4 pulses and bit 1 with 8 pulses.
/// </remarks>
public class TzxSymDef
{
    /// <summary>
    /// Bit 0 - Bit 1: Starting symbol polarity
    /// </summary>
    /// <remarks>
    /// 00: opposite to the current level (make an edge, as usual) - default
    /// 01: same as the current level(no edge - prolongs the previous pulse)
    /// 10: force low level
    /// 11: force high level
    /// </remarks>
    public byte SymbolFlags;

    /// <summary>
    /// The array of pulse lengths
    /// </summary>
    public ushort[] PulseLengths;

    public TzxSymDef(byte maxPulses)
    {
        PulseLengths = new ushort[maxPulses];
    }

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public void ReadFrom(BinaryReader reader)
    {
        SymbolFlags = reader.ReadByte();
        PulseLengths = TzxBlockBase.ReadWords(reader, PulseLengths.Length);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(SymbolFlags);
        TzxBlockBase.WriteWords(writer, PulseLengths);
    }
}

/// <summary>
/// Symbol repetitions
/// </summary>
public struct TzxPrle
{
    /// <summary>
    /// Symbol represented
    /// </summary>
    public byte Symbol;

    /// <summary>
    /// Number of repetitions
    /// </summary>
    public ushort Repetitions;
}

/// <summary>
/// This block represents select structure
/// </summary>
public class TzxSelect
{
    /// <summary>
    /// Bit 0 - Bit 1: Starting symbol polarity
    /// </summary>
    /// <remarks>
    /// 00: opposite to the current level (make an edge, as usual) - default
    /// 01: same as the current level(no edge - prolongs the previous pulse)
    /// 10: force low level
    /// 11: force high level
    /// </remarks>
    public ushort BlockOffset;

    /// <summary>
    /// Length of the description
    /// </summary>
    public byte DescriptionLength { get; set; }

    /// <summary>
    /// The description bytes
    /// </summary>
    public byte[]? Description;

    /// <summary>
    /// The string form of description
    /// </summary>
    public string DescriptionText => TzxBlockBase.ToAsciiString(Description!);

    public TzxSelect(byte length)
    {
        DescriptionLength = length;
    }

    /// <summary>
    /// Reads the content of the block from the specified binary stream.
    /// </summary>
    /// <param name="reader">Stream to read the block from</param>
    public void ReadFrom(BinaryReader reader)
    {
        BlockOffset = reader.ReadUInt16();
        DescriptionLength = reader.ReadByte();
        Description = reader.ReadBytes(DescriptionLength);
    }

    /// <summary>
    /// Writes the content of the block to the specified binary stream.
    /// </summary>
    /// <param name="writer">Stream to write the block to</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(BlockOffset);
        writer.Write(DescriptionLength);
        writer.Write(Description!);
    }
}

/// <summary>
/// Identified AD or DA converter types
/// </summary>
public enum TzxAdOrDaConverterType : byte
{
    HarleySystemsAdc8P2 = 0x00,
    BlackboardElectronics = 0x01
}

/// <summary>
/// Identified computer types
/// </summary>
public enum TzxComputerType: byte
{
    ZxSpectrum16 = 0x00,
    ZxSpectrum48OrPlus = 0x01,
    ZxSpectrum48Issue1 = 0x02,
    ZxSpectrum128 = 0x03,
    ZxSpectrum128P2 = 0x04,
    ZxSpectrum128P2AOr3 = 0x05,
    Tc2048 = 0x06,
    Ts2068 = 0x07,
    Pentagon128 = 0x08,
    SamCoupe = 0x09,
    DidaktikM = 0x0A,
    DidaktikGama = 0x0B,
    Zx80 = 0x0C,
    Zx81 = 0x0D,
    ZxSpectrum128Spanish = 0x0E,
    ZxSpectrumArabic = 0x0F,
    Tk90X = 0x10,
    Tk95 = 0x11,
    Byte = 0x12,
    Elwro800D3 = 0x13,
    ZsScorpion256 = 0x14,
    AmstradCpc464 = 0x15,
    AmstradCpc664 = 0x16,
    AmstradCpc6128 = 0x17,
    AmstradCpc464P = 0x18,
    AmstradCpc6128P = 0x19,
    JupiterAce = 0x1A,
    Enterprise = 0x1B,
    Commodore64 = 0x1C,
    Commodore128 = 0x1D,
    InvesSpectrumP = 0x1E,
    Profi = 0x1F,
    GrandRomMax = 0x20,
    Kay1024 = 0x21,
    IceFelixHc91 = 0x22,
    IceFelixHc2000 = 0x23,
    AmaterskeRadioMistrum = 0x24,
    Quorum128 = 0x25,
    MicroArtAtm = 0x26,
    MicroArtAtmTurbo2 = 0x27,
    Chrome = 0x28,
    ZxBadaloc = 0x29,
    Ts1500 = 0x2A,
    Lambda = 0x2B,
    Tk65 = 0x2C,
    Zx97 = 0x2D
}

/// <summary>
/// Identified digitizer types
/// </summary>
public enum TzxDigitizerType : byte
{
    RdDigitalTracer = 0x00,
    DkTronicsLightPen = 0x01,
    MicrographPad = 0x02,
    RomnticRobotVideoface = 0x03
}

/// <summary>
/// Identified EPROM programmer types
/// </summary>
public enum TzxEpromProgrammerType : byte
{
    OrmeElectronics = 0x00
}

/// <summary>
/// Identified external storage types
/// </summary>
public enum TzxExternalStorageType : byte
{
    ZxMicroDrive = 0x00,
    OpusDiscovery = 0x01,
    MgtDisciple = 0x02,
    MgtPlusD = 0x03,
    RobotronicsWafaDrive = 0x04,
    TrDosBetaDisk = 0x05,
    ByteDrive = 0x06,
    Watsford = 0x07,
    Fiz = 0x08,
    Radofin = 0x09,
    DidaktikDiskDrive = 0x0A,
    BsDos = 0x0B,
    ZxSpectrumP3DiskDrive = 0x0C,
    JloDiskInterface = 0x0D,
    TimexFdd3000 = 0x0E,
    ZebraDiskDrive = 0x0F,
    RamexMillenia = 0x10,
    Larken = 0x11,
    KempstonDiskInterface = 0x12,
    Sandy = 0x13,
    ZxSpectrumP3EHardDisk = 0x14,
    ZxAtaSp = 0x15,
    DivIde = 0x16,
    ZxCf = 0x17
}

/// <summary>
/// Identified graphics types
/// </summary>
public enum TzxGraphicsType : byte
{
    WrxHiRes = 0x00,
    G007 = 0x01,
    Memotech = 0x02,
    LambdaColour = 0x03
}

/// <summary>
/// Represents the hardware types that can be defined
/// </summary>
public enum TzxHwType: byte
{
    Computer = 0x00,
    ExternalStorage = 0x01,
    RomOrRamTypeAddOn = 0x02,
    SoundDevice = 0x03,
    Joystick = 0x04,
    Mouse = 0x05,
    OtherController = 0x06,
    SerialPort = 0x07,
    ParallelPort = 0x08,
    Printer = 0x09,
    Modem = 0x0A,
    Digitizer = 0x0B,
    NetworkAdapter = 0x0C,
    Keyboard = 0x0D,
    AdOrDaConverter = 0x0E,
    EpromProgrammer = 0x0F,
    Graphics = 0x10
}

/// <summary>
/// Identified joystick types
/// </summary>
public enum TzxJoystickType
{
    Kempston = 0x00,
    ProtekCursor = 0x01,
    Sinclair2Left = 0x02,
    Sinclair1Right = 0x03,
    Fuller = 0x04
}

/// <summary>
/// Identified keyboard and keypad types
/// </summary>
public enum TzxKeyboardType : byte
{
    KeypadForZxSpectrum128K = 0x00
}

/// <summary>
/// Identified modem types
/// </summary>
public enum TzxModemTypes : byte
{
    PrismVtx5000 = 0x00,
    Westridge2050 = 0x01
}

/// <summary>
/// Identified mouse types
/// </summary>
public enum TzxMouseType : byte
{
    AmxMouse = 0x00,
    KempstonMouse = 0x01
}

/// <summary>
/// Identified network adapter types
/// </summary>
public enum TzxNetworkAdapterType : byte
{
    ZxInterface1 = 0x00
}

/// <summary>
/// Identified other controller types
/// </summary>
public enum TzxOtherControllerType : byte
{
    Trisckstick = 0x00,
    ZxLightGun = 0x01,
    ZebraGraphicTablet = 0x02,
    DefnederLightGun = 0x03
}

/// <summary>
/// Identified parallel port types
/// </summary>
public enum TzxParallelPortType : byte
{
    KempstonS = 0x00,
    KempstonE = 0x01,
    ZxSpectrum3P = 0x02,
    Tasman = 0x03,
    DkTronics = 0x04,
    Hilderbay = 0x05,
    InesPrinterface = 0x06,
    ZxLprintInterface3 = 0x07,
    MultiPrint = 0x08,
    OpusDiscovery = 0x09,
    Standard8255 = 0x0A
}

/// <summary>
/// Identified printer types
/// </summary>
public enum TzxPrinterType : byte
{
    ZxPrinter = 0x00,
    GenericPrinter = 0x01,
    EpsonCompatible = 0x02
}

/// <summary>
/// Identifier ROM or RAM add-on types
/// </summary>
public enum TzxRomRamAddOnType : byte
{
    SamRam = 0x00,
    MultifaceOne = 0x01,
    Multiface128K = 0x02,
    MultifaceP3 = 0x03,
    MultiPrint = 0x04,
    Mb02 = 0x05,
    SoftRom = 0x06,
    Ram1K = 0x07,
    Ram16K = 0x08,
    Ram48K = 0x09,
    Mem8To16KUsed = 0x0A
}

/// <summary>
/// Identified serial port types
/// </summary>
public enum TzxSerialPortType : byte
{
    ZxInterface1 = 0x00,
    ZxSpectrum128 = 0x01
}

/// <summary>
/// Identified sound device types
/// </summary>
public enum TzxSoundDeviceType : byte
{
    ClassicAy = 0x00,
    FullerBox = 0x01,
    CurrahMicroSpeech = 0x02,
    SpectDrum = 0x03,
    MelodikAyAcbStereo = 0x04,
    AyAbcStereo = 0x05,
    RamMusinMachine = 0x06,
    Covox = 0x07,
    GeneralSound = 0x08,
    IntecEdiB8001 = 0x09,
    ZonXAy = 0x0A,
    QuickSilvaAy = 0x0B,
    JupiterAce = 0x0C
}

