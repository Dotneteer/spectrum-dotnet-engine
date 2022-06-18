namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents a data block that the tape device can play
/// </summary>
public class TapeDataBlock
{
    /// <summary>
    /// Pilot pulse length
    /// </summary>
    public const int PILOT_PL = 2168;

    /// <summary>
    /// Pilot pulses in the ROM header block
    /// </summary>
    public const int HEADER_PILOT_COUNT = 8063;

    /// <summary>
    /// Pilot pulses in the ROM data block
    /// </summary>
    public const int DATA_PILOT_COUNT = 3223;

    /// <summary>
    /// Sync 1 pulse length
    /// </summary>
    public const int SYNC_1_PL = 667;

    /// <summary>
    /// Sync 2 pulse lenth
    /// </summary>
    public const int SYNC_2_PL = 735;

    /// <summary>
    /// Bit 0 pulse length
    /// </summary>
    public const int BIT_0_PL = 855;

    /// <summary>
    /// Bit 1 pulse length
    /// </summary>
    public const int BIT_1_PL = 1710;

    /// <summary>
    /// End sync pulse length
    /// </summary>
    public const int TERM_SYNC = 947;

    /// <summary>
    /// 1 millisecond pause
    /// </summary>
    public const int PAUSE_MS = 1000;

    /// <summary>
    /// Block Data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Pause after this block (given in milliseconds)
    /// </summary>
    public ushort PauseAfter { get; set; } = 1000;

    /// <summary>
    /// Length of pilot pulse
    /// </summary>
    public ushort PilotPulseLength { get; set; } = PILOT_PL;

    /// <summary>
    /// Length of the first sync pulse
    /// </summary>
    public ushort Sync1PulseLength { get; set; } = SYNC_1_PL;

    /// <summary>
    /// Length of the second sync pulse
    /// </summary>
    public ushort Sync2PulseLength { get; set; } = SYNC_2_PL;

    /// <summary>
    /// Length of the zero bit
    /// </summary>
    public ushort ZeroBitPulseLength { get; set; } = BIT_0_PL;

    /// <summary>
    /// Length of the one bit
    /// </summary>
    public ushort OneBitPulseLength { get; set; } = BIT_1_PL;

    /// <summary>
    /// Lenght of ther end sync pulse
    /// </summary>
    public ushort EndSyncPulseLenght { get; set; } = TERM_SYNC;
}

