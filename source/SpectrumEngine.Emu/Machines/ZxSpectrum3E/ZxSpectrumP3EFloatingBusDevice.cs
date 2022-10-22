namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the ZX Spectrum +3E floating bus device.
/// </summary>
public class ZxSpectrumP3EFloatingBusDevice: IFloatingBusDevice
{
    private Func<bool> _pagingEnabledFunc;
    /// <summary>
    /// Initialize the floating port device and assign it to its host machine.
    /// </summary>
    /// <param name="machine">The machine hosting this device</param>
    public ZxSpectrumP3EFloatingBusDevice(ZxSpectrumP3EMachine machine)
    {
        Machine = machine;
        _pagingEnabledFunc = () => machine.MemoryPagingEnabled;
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
        // --- Floating bus works only when memory paging is enabled
        if (!_pagingEnabledFunc()) return 0xff;
        /*
        var screen = Machine.ScreenDevice;
        var renderTact = (Machine.CurrentFrameTact - 3 + Machine.TactsInFrame) % Machine.TactsInFrame;
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
    */
        return 0xff;
    }
}