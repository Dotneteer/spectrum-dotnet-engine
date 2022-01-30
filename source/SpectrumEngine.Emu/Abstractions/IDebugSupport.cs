﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This interface represents the properties and methods that support debugging an emulated machine.
/// </summary>
public interface IDebugSupport
{
    /// <summary>
    /// This member stores the last startup breakpoint to check. It allows setting a breakpoint to the first
    /// instruction of a program.
    /// </summary>
    ushort? LastStartupBreakpoint { get; set; }
}
