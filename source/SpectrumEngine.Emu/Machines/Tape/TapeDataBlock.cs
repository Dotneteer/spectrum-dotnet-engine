namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents a data block that the tape device can play
/// </summary>
public class TapeDataBlock
{
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
    public ushort PilotPulseLength { get; set; } = 2168;

    /// <summary>
    /// Length of the first sync pulse
    /// </summary>
    public ushort Sync1PulseLength { get; set; } = 667;

    /// <summary>
    /// Length of the second sync pulse
    /// </summary>
    public ushort Sync2PulseLength { get; set; } = 735;

    /// <summary>
    /// Length of the zero bit
    /// </summary>
    public ushort ZeroBitPulseLength { get; set; } = 855;

    /// <summary>
    /// Length of the one bit
    /// </summary>
    public ushort OneBitPulseLength { get; set; } = 1710;

    /// <summary>
    /// Lenght of ther end sync pulse
    /// </summary>
    public ushort EndSyncPulseLenght { get; set; } = 947;
  
    
}

