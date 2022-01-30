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
    public void SetTactsInFrame(int tacts)
    {
        TactsInFrame = tacts;
    }

    /// <summary>
    /// Reset the flag that indicates the machine frame completion.
    /// </summary>
    public void ResetFrameCompletedFlag()
    {
        FrameCompleted = false;
    }

    /// <summary>
    /// This method increments the current CPU tacts by one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus1()
    {
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by two.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus2()
    {
        IncrementTacts();
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by three.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus3()
    {
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by four.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus4()
    {
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by five.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus5()
    {
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by six.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus6()
    {
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by seven.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus7()
    {
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
        IncrementTacts();
    }

    /// <summary>
    /// This method increments the current CPU tacts by N.
    /// </summary>
    /// <param name="n">The number to increate the CPU tacts by</param>
    public void TactPlusN(byte n)
    {
        for (int i = 0; i < n; i++)
        {
            IncrementTacts();
        }
    }

    /// <summary>
    /// Increments the current CPU tacts
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementTacts()
    {
        if (++CurrentFrameTact >= TactsInFrame * ClockMultiplier)
        {
            CurrentFrameTact = 0;
            Frames++;
            FrameCompleted = true;
        }
        TactIncrementedHandler();
    }
}