namespace SpectrumEngine.Emu;

/// <summary>
/// This class manages the ZX Spectrum devices when the CPU reads or writes I/O ports.
/// </summary>
public class ZxSpectrum48IoHandler : IIoHandler<IZxSpectrum48Machine>
{
    // --- Last value of bit 3 on port $FE
    private bool _portBit3LastValue;

    // --- Last value of bit 4 on port $FE
    private bool _portBit4LastValue;

    // --- Tacts value when last time bit 4 of $fe changed from 0 to 1
    private ulong _portBit4ChangedFrom0Tacts;

    // --- Tacts value when last time bit 4 of $fe changed from 1 to 0
    private ulong _portBit4ChangedFrom1Tacts;

    /// <summary>
    /// Initialize the I/O handler and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48IoHandler(IZxSpectrum48Machine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Get the machine that hosts the I/O handler
    /// </summary>
    public IZxSpectrum48Machine Machine { get; }

    /// <summary>
    /// Reset the I/O to its initial state.
    /// </summary>
    public void Reset()
    {
    }

    /// <summary>
    /// Read a byte from the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <returns>The byte read from the port</returns>
    public byte ReadPort(ushort address)
    {
        return (address & 0x0001) == 0
            ? ReadPort0xFE(address)
            : Machine.FloatingBusDevice.ReadFloatingPort();
    }

    /// <summary>
    /// Write the given byte to the specified I/O port.
    /// </summary>
    /// <param name="address">16-bit I/O port address</param>
    /// <param name="value">Byte to write to the I/O port</param>
    public void WritePort(ushort address, byte value)
    {
        if ((address & 0x0001) == 0)
        {
            WritePort0xFE(value);
        }
    }

    /// <summary>
    /// Reads a byte from the ZX Spectrum generic input port.
    /// </summary>
    /// <param name="address">Port address</param>
    /// <returns>Byte value read from the generic port</returns>
    private byte ReadPort0xFE(ushort address)
    {
        var portValue = Machine.KeyboardDevice.GetKeyLineStatus(address);
        bool earBit;
        bool bit4Sensed;

        // --- Check for LOAD mode
        if (Machine.TapeDevice.TapeMode == TapeMode.Load)
        {
            earBit = Machine.TapeDevice.GetTapeEarBit();
            Machine.BeeperDevice.SetEarBit(earBit);
            portValue = (byte)((portValue & 0xbf) | (earBit ? 0x40 : 0));
        }
        else
        {
            // --- Handle analog EAR bit
            bit4Sensed = _portBit4LastValue;
            if (!bit4Sensed)
            {
                // --- Changed later to 1 from 0 than to 0 from 1?
                var chargeTime = _portBit4ChangedFrom1Tacts - _portBit4ChangedFrom0Tacts;
                if (chargeTime > 0)
                {
                    // --- Yes, calculate charge time
                    chargeTime = chargeTime > 700 ? 2800 : 4 * chargeTime;

                    // --- Calculate time ellapsed since last change from 1 to 0
                    bit4Sensed = Machine.Cpu.Tacts - _portBit4ChangedFrom1Tacts < chargeTime;
                }
            }

            // --- Calculate bit 6 value
            var bit6Value = _portBit3LastValue
              ? 0x40
              : bit4Sensed
                ? 0x40
                : 0x00;

            // --- Check for ULA 3
            if (Machine.UlaIssue == 3 && _portBit3LastValue && !bit4Sensed)
            {
                bit6Value = 0x00;
            }

            // --- Merge bit 6 with port value
            portValue = (byte)((portValue & 0xbf) | bit6Value);
        }
        return portValue;
    }

    /// <summary>
    /// Wites the specified data byte to the ZX Spectrum generic output port.
    /// </summary>
    /// <param name="address">Port address</param>
    /// <param name="value">Data byte to write</param>
    private void WritePort0xFE(byte value)
    {
        // --- Extract bthe border color
        Machine.ScreenDevice.BorderColor = value & 0x07;

        // --- Store the last EAR bit
        var bit4 = value & 0x10;
        Machine.BeeperDevice.SetEarBit(bit4 != 0);

        // --- Set the last value of bit3
        _portBit3LastValue = (value & 0x08) != 0;

        // --- Instruct the tape device process the MIC bit
        Machine.TapeDevice.ProcessMicBit(_portBit3LastValue);

        // --- Manage bit 4 value
        if (_portBit4LastValue)
        {
            // --- Bit 4 was 1, is it now 0?
            if (bit4 == 0)
            {
                _portBit4ChangedFrom1Tacts = Machine.Cpu.Tacts;
                _portBit4LastValue = false;
            }
        }
        else
        {
            // --- Bit 4 was 0, is it now 1?
            if (bit4 != 0)
            {
                _portBit4ChangedFrom0Tacts = Machine.Cpu.Tacts;
                _portBit4LastValue = true;
            }
        }
    }
}
