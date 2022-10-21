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
        Tacts += 1;
        FrameTacts += 1;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(1);
    }

    /// <summary>
    /// This method increments the current CPU tacts by two.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus2()
    {
        Tacts += 2;
        FrameTacts += 2;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(2);
    }

    /// <summary>
    /// This method increments the current CPU tacts by three.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus3()
    {
        Tacts += 3;
        FrameTacts += 3;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(3);
    }

    /// <summary>
    /// This method increments the current CPU tacts by four.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus4()
    {
        Tacts += 4;
        FrameTacts += 4;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(4);
    }

    /// <summary>
    /// This method increments the current CPU tacts by five.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus5()
    {
        Tacts += 5;
        FrameTacts += 5;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(5);
    }

    /// <summary>
    /// This method increments the current CPU tacts by six.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus6()
    {
        Tacts += 6;
        FrameTacts += 6;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(6);
    }

    /// <summary>
    /// This method increments the current CPU tacts by N.
    /// </summary>
    /// <param name="n">The number to increate the CPU tacts by</param>
    public void TactPlusN(byte n)
    {
        Tacts += n;
        FrameTacts += n;
        var totalTacts = TactsInFrame * ClockMultiplier;
        if (FrameTacts >= totalTacts)
        {
            Frames++;
            FrameTacts -= totalTacts;
        }
        OnTactIncremented(n);
    }
}