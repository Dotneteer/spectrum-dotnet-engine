using Avalonia.Input;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia;

/// <summary>
/// Represents a mapping between physical keys and Spectrum keyboard keys
/// </summary>
/// <param name="Input"></param>
/// <param name="Primary"></param>
/// <param name="Secondary"></param>
public record KeyMapping(Key Input, SpectrumKeyCode Primary, SpectrumKeyCode? Secondary = null);

/// <summary>
/// This class manages the mappings of physical key presses to ZX Spectrum keys
/// </summary>
public static class KeyMappings
{
    public static readonly KeyMapping[] DefaultMapping =
    {
        // --- First ZX Spectrum key row
        new(Key.D1, SpectrumKeyCode.N1),
        new(Key.NumPad1, SpectrumKeyCode.N1),
        new(Key.D2, SpectrumKeyCode.N2),
        new(Key.NumPad2, SpectrumKeyCode.N2),
        new(Key.D3, SpectrumKeyCode.N3),
        new(Key.NumPad3, SpectrumKeyCode.N3),
        new(Key.D4, SpectrumKeyCode.N4),
        new(Key.NumPad4, SpectrumKeyCode.N4),
        new(Key.D5, SpectrumKeyCode.N5),
        new(Key.NumPad5, SpectrumKeyCode.N5),
        new(Key.Left, SpectrumKeyCode.N5, SpectrumKeyCode.CShift),
        new(Key.D6, SpectrumKeyCode.N6),
        new(Key.NumPad6, SpectrumKeyCode.N6),
        new(Key.Down, SpectrumKeyCode.N6, SpectrumKeyCode.CShift),
        new(Key.D7, SpectrumKeyCode.N7),
        new(Key.NumPad7, SpectrumKeyCode.N7),
        new(Key.Up, SpectrumKeyCode.N7, SpectrumKeyCode.CShift),
        new(Key.D8, SpectrumKeyCode.N8),
        new(Key.NumPad8, SpectrumKeyCode.N8),
        new(Key.Right, SpectrumKeyCode.N8, SpectrumKeyCode.CShift),
        new(Key.D9, SpectrumKeyCode.N9),
        new(Key.NumPad9, SpectrumKeyCode.N9),
        new(Key.D0, SpectrumKeyCode.N0),
        new(Key.NumPad0, SpectrumKeyCode.N0),

        // --- Second ZX Spectrum key row
        new(Key.Q, SpectrumKeyCode.Q),
        new(Key.W, SpectrumKeyCode.W),
        new(Key.E, SpectrumKeyCode.E),
        new(Key.R, SpectrumKeyCode.R),
        new(Key.T, SpectrumKeyCode.T),
        new(Key.Y, SpectrumKeyCode.Y),
        new(Key.U, SpectrumKeyCode.U),
        new(Key.I, SpectrumKeyCode.I),
        new(Key.O, SpectrumKeyCode.O),
        new(Key.P, SpectrumKeyCode.P),

        // --- Third ZX Spectrum key row
        new(Key.A, SpectrumKeyCode.A),
        new(Key.S, SpectrumKeyCode.S),
        new(Key.D, SpectrumKeyCode.D),
        new(Key.F, SpectrumKeyCode.F),
        new(Key.G, SpectrumKeyCode.G),
        new(Key.H, SpectrumKeyCode.H),
        new(Key.J, SpectrumKeyCode.J),
        new(Key.K, SpectrumKeyCode.K),
        new(Key.L, SpectrumKeyCode.L),
        new(Key.Enter, SpectrumKeyCode.Enter),

        // --- Fourth ZX Spectrum key row
        new(Key.LeftShift, SpectrumKeyCode.CShift),
        new(Key.RightShift, SpectrumKeyCode.CShift),
        new(Key.Z, SpectrumKeyCode.Z),
        new(Key.X, SpectrumKeyCode.X),
        new(Key.D, SpectrumKeyCode.D),
        new(Key.C, SpectrumKeyCode.C),
        new(Key.V, SpectrumKeyCode.V),
        new(Key.B, SpectrumKeyCode.B),
        new(Key.N, SpectrumKeyCode.N),
        new(Key.M, SpectrumKeyCode.M),
        new(Key.RightAlt, SpectrumKeyCode.SShift),
        new(Key.Space, SpectrumKeyCode.Space),

        // --- Extra key combinations
        new(Key.Back, SpectrumKeyCode.N0, SpectrumKeyCode.CShift),
        new(Key.Home, SpectrumKeyCode.N1, SpectrumKeyCode.CShift),
    };
}
