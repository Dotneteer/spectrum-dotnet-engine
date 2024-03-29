﻿using System.Runtime.CompilerServices;

namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition defines members that handle memory contention.
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This flag indicates whether the Z80 CPU works in hardware with ULA-controlled memory contention between the
    /// CPU and other components.
    /// </summary>
    public virtual bool DelayedAddressBus { get; set; }

    /// <summary>
    /// This method increments the current CPU tacts using memory contention with the provided address.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus1(ushort address)
    {
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
    }

    /// <summary>
    /// This method increments the current CPU tacts by two, using memory contention with the provided address.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus2(ushort address)
    {
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
    }

    /// <summary>
    /// This method increments the current CPU tacts by four, using memory contention with the provided address.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus4(ushort address)
    {
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
    }


    /// <summary>
    /// This method increments the current CPU tacts by five, using memory contention with the provided address.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus5(ushort address)
    {
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
    }

    /// <summary>
    /// This method increments the current CPU tacts by seven, using memory contention with the provided address.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TactPlus7(ushort address)
    {
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
        if (DelayedAddressBus) DelayAddressBusAccess(address);
        TactPlus1();
    }
}