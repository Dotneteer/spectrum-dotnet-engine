namespace SpectrumEngine.Emu.Machines.ZxSpectrum128;

public class ZxSpectrum128PsgDevice: AudioDeviceBase, IPsgDevice
{
    /// <summary>
    /// Initialize the beeper device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum128PsgDevice(IZxSpectrumMachine machine) : base(machine)
    {
    }

    /// <summary>
    /// Calculates the current audio value according to the CPU's clock
    /// </summary>
    /// <remarks>
    /// We do not need to calculate the value, as it is always the value of the last EAR bit
    /// </remarks>
    protected override void CalculateCurrentAudioValue()
    {
        // TODO: 
    }

    /// <summary>
    /// Gets the current sound sample (according to the current CPU tact)
    /// </summary>
    /// <returns>Sound sample value</returns>
    protected override float GetCurrentSampleValue()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Sets the PSG register index to read or write
    /// </summary>
    /// <param name="index">PSG register index</param>
    public void SetPsgRegisterIndex(int index)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads the value of the PSG register addressed with the las SetPsgRegisterIndex operation
    /// </summary>
    /// <returns>The value of the PSG register</returns>
    public byte ReadPsgRegister()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes the value of the PSG register addressed with the las SetPsgRegisterIndex operation
    /// </summary>
    /// <param name="value">Value to set for the PSG register</param>
    public void WritePsgRegisterValue(byte value)
    {
        throw new NotImplementedException();
    }
}