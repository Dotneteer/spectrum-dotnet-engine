﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum 48 floating bus device.
/// </summary>
public sealed class ZxSpectrum48FloatingBusDevice : IFloatingBusDevice
{
    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrum48FloatingBusDevice(IZxSpectrumMachine machine)
    {
        Machine = machine;
    }

    /// <summary>
    /// Release resources
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Get the machine that hosts the device.
    /// </summary>
    public IZxSpectrumMachine Machine { get; }

    /// <summary>
    /// Reset the device to its initial state.
    /// </summary>
    public void Reset()
    {
        // --- Intentionally empty
    }

    /// <summary>
    /// Reads the current floating bus value.
    /// </summary>
    /// <returns></returns>
    public byte ReadFloatingBus()
    {
        var screen = Machine.ScreenDevice;
        var renderTact = (Machine.CurrentFrameTact - 5 + Machine.TactsInFrame) % Machine.TactsInFrame;
        var renderingTact = screen.RenderingTactTable[renderTact];
        switch (renderingTact.Phase)
        {
            case RenderingPhase.BorderFetchPixel:
            case RenderingPhase.DisplayB1FetchB2:
            case RenderingPhase.DisplayB2FetchB1:
                return Machine.ReadScreenMemory(renderingTact.PixelAddress);
            case RenderingPhase.BorderFetchAttr:
            case RenderingPhase.DisplayB1FetchA2:
            case RenderingPhase.DisplayB2FetchA1:
                return Machine.ReadScreenMemory(renderingTact.AttributeAddress);
            default:
                return 0xff;
        }
    }
}

