using SpectrumEngine.Emu;
using System.Windows.Input;

namespace SpectrumEngine.Client.Wpf;

/// <summary>
/// Represents a mapping between physical keys and Spectrum keyboard keys
/// </summary>
/// <param name="Input"></param>
/// <param name="primary"></param>
/// <param name="secondary"></param>
public record KeyMapping(Key Input, SpectrumKeyCode Primary, SpectrumKeyCode? Secondary = null) { }

/// <summary>
/// This class manages the mappings of physical key presses to ZX Spectrum keys
/// </summary>
public static class KeyMappings
{
    public readonly static KeyMapping[] DefaultMapping =
    {
        // --- First ZX Spectrum key row
        new KeyMapping(Key.D1, SpectrumKeyCode.N1),
        new KeyMapping(Key.NumPad1, SpectrumKeyCode.N1),
        new KeyMapping(Key.D2, SpectrumKeyCode.N2),
        new KeyMapping(Key.NumPad2, SpectrumKeyCode.N2),
        new KeyMapping(Key.D3, SpectrumKeyCode.N3),
        new KeyMapping(Key.NumPad3, SpectrumKeyCode.N3),
        new KeyMapping(Key.D4, SpectrumKeyCode.N4),
        new KeyMapping(Key.NumPad4, SpectrumKeyCode.N4),
        new KeyMapping(Key.D5, SpectrumKeyCode.N5),
        new KeyMapping(Key.NumPad5, SpectrumKeyCode.N5),
        new KeyMapping(Key.D6, SpectrumKeyCode.N6),
        new KeyMapping(Key.NumPad6, SpectrumKeyCode.N6),
        new KeyMapping(Key.D7, SpectrumKeyCode.N7),
        new KeyMapping(Key.NumPad7, SpectrumKeyCode.N7),
        new KeyMapping(Key.D8, SpectrumKeyCode.N8),
        new KeyMapping(Key.NumPad8, SpectrumKeyCode.N8),
        new KeyMapping(Key.D9, SpectrumKeyCode.N9),
        new KeyMapping(Key.NumPad9, SpectrumKeyCode.N9),
        new KeyMapping(Key.D0, SpectrumKeyCode.N0),
        new KeyMapping(Key.NumPad0, SpectrumKeyCode.N0),

        // --- Second ZX Spectrum key row
        new KeyMapping(Key.Q, SpectrumKeyCode.Q),
        new KeyMapping(Key.W, SpectrumKeyCode.W),
        new KeyMapping(Key.E, SpectrumKeyCode.E),
        new KeyMapping(Key.R, SpectrumKeyCode.R),
        new KeyMapping(Key.T, SpectrumKeyCode.T),
        new KeyMapping(Key.Y, SpectrumKeyCode.Y),
        new KeyMapping(Key.U, SpectrumKeyCode.U),
        new KeyMapping(Key.I, SpectrumKeyCode.I),
        new KeyMapping(Key.O, SpectrumKeyCode.O),
        new KeyMapping(Key.P, SpectrumKeyCode.P),

        // --- Third ZX Spectrum key row
        new KeyMapping(Key.A, SpectrumKeyCode.A),
        new KeyMapping(Key.S, SpectrumKeyCode.S),
        new KeyMapping(Key.D, SpectrumKeyCode.D),
        new KeyMapping(Key.F, SpectrumKeyCode.F),
        new KeyMapping(Key.G, SpectrumKeyCode.G),
        new KeyMapping(Key.H, SpectrumKeyCode.H),
        new KeyMapping(Key.J, SpectrumKeyCode.J),
        new KeyMapping(Key.K, SpectrumKeyCode.K),
        new KeyMapping(Key.L, SpectrumKeyCode.L),
        new KeyMapping(Key.Enter, SpectrumKeyCode.Enter),

        // --- Fourth ZX Spectrum key row
        new KeyMapping(Key.LeftShift, SpectrumKeyCode.CShift),
        new KeyMapping(Key.RightShift, SpectrumKeyCode.CShift),
        new KeyMapping(Key.Z, SpectrumKeyCode.Z),
        new KeyMapping(Key.X, SpectrumKeyCode.X),
        new KeyMapping(Key.D, SpectrumKeyCode.D),
        new KeyMapping(Key.C, SpectrumKeyCode.C),
        new KeyMapping(Key.V, SpectrumKeyCode.V),
        new KeyMapping(Key.B, SpectrumKeyCode.B),
        new KeyMapping(Key.N, SpectrumKeyCode.N),
        new KeyMapping(Key.M, SpectrumKeyCode.M),
        new KeyMapping(Key.RightAlt, SpectrumKeyCode.SShift),
        new KeyMapping(Key.Space, SpectrumKeyCode.Space),

        // --- Extra key combinations
        new KeyMapping(Key.Back, SpectrumKeyCode.N0, SpectrumKeyCode.CShift),
        new KeyMapping(Key.Home, SpectrumKeyCode.N1, SpectrumKeyCode.CShift),
    };
}
