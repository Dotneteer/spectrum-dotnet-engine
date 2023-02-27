using static SpectrumEngine.Emu.Machines.Disk.Controllers.NecUpd765;

namespace SpectrumEngine.Emu.Machines.Disk.Controllers;


/// <summary>
/// Class that holds configuration about a specific command
/// </summary>
public record CommandConfiguration
{
    /// <summary>
    /// The command code after bitmask has been applied
    /// </summary>
    public CommandCode CommandCode { get; init; } = default!;

    /// <summary>
    /// Command handler
    /// </summary>
    public Action CommandHandler { get; init; } = default!;

    /// <summary>
    /// The number of bytes that make up the full command
    /// </summary>
    public int ParameterBytesCount { get; init; }

    /// <summary>
    /// The number of result bytes that will be generated from the command
    /// </summary>
    public int ResultBytesCount { get; init; }

    /// <summary>
    /// Command flow
    /// IN - Z80 to UPD765A
    /// OUT - UPD765A to Z80
    /// </summary>
    public CommandFlow CommandFlow { get; init; }

    /// <summary>
    /// Command oeration Read/Write
    /// </summary>
    public CommandOperation CommandOperation { get; init; }

    /// <summary>
    /// Command flags
    /// </summary>
    public CommandFlags CommandFlags { get; init; }
}

/// <summary>
/// Floppy disk drive controller command.
/// </summary>
public record Command
{
    /// <summary>
    /// Track
    /// </summary>
    public byte C { get; init; }

    /// <summary>
    /// Side
    /// </summary>
    public byte H { get; init; }

    /// <summary>
    /// Sector ID
    /// </summary>
    public byte R { get; init; }

    /// <summary>
    /// Sector Size
    /// </summary>
    public byte N { get; init; }

    /// <summary>
    /// Status register 0
    /// </summary>
    /// <remarks>
    /// bits description:
    /// 
    /// b0,1    US Unit Select(driveno during interrupt)
    /// b2      HD  Head Address(head during interrupt)
    /// b3      NR  Not Ready(drive not ready or non-existing 2nd head selected)
    /// b4      EC  Equipment Check(drive failure or recalibrate failed (retry))
    /// b5      SE  Seek End(Set if seek-command completed)
    /// b6,7    IC Interrupt Code(0=OK, 1=aborted:readfail/OK if EN, 2=unknown cmd
    ///         or senseint with no int occured, 3=aborted:disc removed etc.)
    /// </remarks>
    public byte St0 { get; init; }

    /// <summary>
    /// Status register 1
    /// </summary>
    /// <remarks>
    /// bits description:
    /// 
    /// b0      MA  Missing Address Mark(Sector_ID or DAM not found)
    /// b1      NW  Not Writeable(tried to write/format disc with wprot_tab = on)
    /// b2      ND  No Data(Sector_ID not found, CRC fail in ID_field)
    /// b3,6    0   Not used
    /// b4      OR  Over Run(CPU too slow in execution-phase (ca. 26us/Byte))
    /// b5      DE  Data Error(CRC-fail in ID- or Data-Field)
    /// b7      EN  End of Track(set past most read/write commands) (see IC)
    /// </remarks>
    public byte St1 { get; init; }

    /// <summary>
    /// Status register 2
    /// </summary>
    /// <remarks>
    /// bits description:
    /// 
    /// b0      MD  Missing Address Mark in Data Field(DAM not found)
    /// b1      BC  Bad Cylinder(read/programmed track-ID different and read-ID = FF)
    /// b2      SN  Scan Not Satisfied(no fitting sector found)
    /// b3      SH  Scan Equal Hit(equal)
    /// b4      WC  Wrong Cylinder(read/programmed track-ID different) (see b1)
    /// b5      DD Data Error in Data Field(CRC-fail in data-field)
    /// b6      CM  Control Mark(read/scan command found sector with deleted DAM)
    /// b7      0   Not Used
    /// </remarks>
    public byte St2 { get; init; }

    /// <summary>
    /// Status register 3
    /// </summary>
    /// <remarks>
    /// bits description:
    /// 
    /// b0,1    US Unit Select (pin 28,29 of FDC)
    /// b2      HD  Head Address (pin 27 of FDC)
    /// b3      TS  Two Side (0=yes, 1=no (!))
    /// b4      T0  Track 0 (on track 0 we are)
    /// b5      RY  Ready (drive ready signal)
    /// b6      WP  Write Protected(write protected)
    /// b7      FT  Fault (if supported: 1=Drive failure)
    /// </remarks>
    public byte St3 { get; init; }

    /// <summary>
    /// Last transmitted/received data bytes
    /// </summary>
    public byte[]? DataBytes { get; init; }

    /// <summary>
    /// read/write data command ID
    /// </summary>
    public int DataId { get; init; }
}

/// <summary>
/// Command common flags
/// </summary>
public struct CommandFlags
{
    /// <summary>
    /// Use of the MT bit (Multitrack bit)
    /// </summary>
    public bool MT { get; init; }

    /// <summary>
    /// Use of the MF bit (Multi Format Single/double density)
    /// </summary>
    public bool MF { get; init; }

    /// <summary>
    /// Use of the SK bit (Skip bit)
    /// </summary>
    public bool SK { get; init; }
}

/// <summary>
/// Floppy disk drive controller command data
/// </summary>
public struct CommandData
{
    /// <summary>
    /// The requested drive
    /// </summary>
    public byte UnitSelect;
    /// <summary>
    /// The requested physical side
    /// </summary>
    public byte Side;
    /// <summary>
    /// The requested track (C)
    /// </summary>
    public byte Cylinder;
    /// <summary>
    /// The requested head (H)
    /// </summary>
    public byte Head;
    /// <summary>
    /// The requested sector (R)
    /// </summary>
    public byte Sector;
    /// <summary>
    /// The specified sector size (N)
    /// </summary>
    public byte SectorSize;
    /// <summary>
    /// The end of track or last sector value (EOT)
    /// </summary>
    public byte EOT;
    /// <summary>
    /// Gap3 length (GPL)
    /// </summary>
    public byte Gap3Length;
    /// <summary>
    /// Data length (DTL) - When N is defined as 00, DTL stands for the data length 
    /// which users are going to read out or write into the sector
    /// </summary>
    public byte DTL;
}
