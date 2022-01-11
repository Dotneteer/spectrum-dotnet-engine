namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains methods that handle the clock and CPU timing.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This method increments the current CPU tacts by one.
    /// </summary>
    public void TactPlus1()
    {
        Tacts++;
        TactIncrementedHandler();
    }

    /// <summary>
    /// This method increments the current CPU tacts by two.
    /// </summary>
    public void TactPlus2()
    {
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
    }

    /// <summary>
    /// This method increments the current CPU tacts by three.
    /// </summary>
    public void TactPlus3()
    {
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
    }

    /// <summary>
    /// This method increments the current CPU tacts by four.
    /// </summary>
    public void TactPlus4()
    {
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
    }

    /// <summary>
    /// This method increments the current CPU tacts by five.
    /// </summary>
    public void TactPlus5()
    {
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
    }

    /// <summary>
    /// This method increments the current CPU tacts by seven.
    /// </summary>
    public void TactPlus7()
    {
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
        Tacts++;
        TactIncrementedHandler();
    }

    /// <summary>
    /// This method increments the current CPU tacts by N.
    /// </summary>
    /// <param name="n">The number to increate the CPU tacts by</param>
    public void TactPlusN(byte n)
    {
        for (int i = 0; i < n; i++)
        {
            Tacts++;
            TactIncrementedHandler();
        }
    }
}