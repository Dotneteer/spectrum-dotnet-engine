using System.Runtime.CompilerServices;

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
    /// Set the number of tacts in a machine frame.
    /// </summary>
    /// <param name="tacts">Number of tacts in a machine frame</param>
    public virtual void SetTactsInFrame(int tacts)
    {
        TactsInFrame = tacts;
    }

    /// <summary>
    /// This method increments the current CPU tacts by one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus1()
    {
        var oldTacts = Tacts;
        Tacts += 1;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by two.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus2()
    {
        var oldTacts = Tacts;
        Tacts += 2;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by three.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus3()
    {
        var oldTacts = Tacts;
        Tacts += 3;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by four.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus4()
    {
        var oldTacts = Tacts;
        Tacts += 4;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by five.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus5()
    {
        var oldTacts = Tacts;
        Tacts += 5;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by six.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus6()
    {
        var oldTacts = Tacts;
        Tacts += 6;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by seven.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus7()
    {
        var oldTacts = Tacts;
        Tacts += 7;
        OnTactIncremented(oldTacts);
    }

    /// <summary>
    /// This method increments the current CPU tacts by N.
    /// </summary>
    /// <param name="n">The number to increate the CPU tacts by</param>
    public void TactPlusN(byte n)
    {
        var oldTacts = Tacts;
        Tacts += n;
        OnTactIncremented(oldTacts);
    }
}