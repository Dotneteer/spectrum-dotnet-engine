using Avalonia.Input;
using SpectrumEngine.Client.Avalonia.Models;
using SpectrumEngine.Emu;
using System.Collections.Generic;
using System.Linq;

namespace SpectrumEngine.Client.Avalonia.Providers
{
    /// <summary>
    /// Class with spectrum 48 keyboard provider
    /// </summary>
    public class ZxSpectrum48Provider : IKeyboardProvider
    {
        private readonly IDictionary<Key, IEnumerable<SpectrumKeyCode>> keyMapping;

        public ZxSpectrum48Provider()
        {
            keyMapping = CreateKeyMapping();
        }

        public Machine Machine => Machine.ZxSpectrum48;

        public IEnumerable<SpectrumKeyCode> MapKey(Key key) => keyMapping.ContainsKey(key) ? keyMapping[key] : Enumerable.Empty<SpectrumKeyCode>();

        private static IDictionary<Key, IEnumerable<SpectrumKeyCode>> CreateKeyMapping()
        {
            return new Dictionary<Key, IEnumerable<SpectrumKeyCode>>
            {
                // --- First ZX Spectrum key row
                { Key.D1, new[] { SpectrumKeyCode.N1}},
                { Key.NumPad1, new[] { SpectrumKeyCode.N1 }},
                { Key.D2, new[] { SpectrumKeyCode.N2 }},
                { Key.NumPad2, new[] { SpectrumKeyCode.N2 }},
                { Key.D3, new[] { SpectrumKeyCode.N3 }},
                { Key.NumPad3, new[] { SpectrumKeyCode.N3 }},
                { Key.D4, new[] { SpectrumKeyCode.N4 }},
                { Key.NumPad4, new[] { SpectrumKeyCode.N4 }},
                { Key.D5, new[] { SpectrumKeyCode.N5 }},
                { Key.NumPad5, new[] { SpectrumKeyCode.N5 }},
                { Key.D6, new[] { SpectrumKeyCode.N6 }},
                { Key.NumPad6, new[] { SpectrumKeyCode.N6 }},
                { Key.D7, new[] { SpectrumKeyCode.N7 }},
                { Key.NumPad7, new[] { SpectrumKeyCode.N7 }},
                { Key.D8, new[] { SpectrumKeyCode.N8 }},
                { Key.NumPad8, new[] { SpectrumKeyCode.N8 }},
                { Key.D9, new[] { SpectrumKeyCode.N9 }},
                { Key.NumPad9, new[] { SpectrumKeyCode.N9 }},
                { Key.D0, new[] { SpectrumKeyCode.N0 }},
                { Key.NumPad0, new[] { SpectrumKeyCode.N0 }},

                // --- Second ZX Spectrum key row
                { Key.Q, new[] { SpectrumKeyCode.Q }},
                { Key.W, new[] { SpectrumKeyCode.W }},
                { Key.E, new[] { SpectrumKeyCode.E }},
                { Key.R, new[] { SpectrumKeyCode.R }},
                { Key.T, new[] { SpectrumKeyCode.T }},
                { Key.Y, new[] { SpectrumKeyCode.Y }},
                { Key.U, new[] { SpectrumKeyCode.U }},
                { Key.I, new[] { SpectrumKeyCode.I }},
                { Key.O, new[] { SpectrumKeyCode.O }},
                { Key.P, new[] { SpectrumKeyCode.P }},

                // --- Third ZX Spectrum key row
                { Key.A, new[] { SpectrumKeyCode.A }},
                { Key.S, new[] { SpectrumKeyCode.S }},
                { Key.D, new[] { SpectrumKeyCode.D }},
                { Key.F, new[] { SpectrumKeyCode.F }},
                { Key.G, new[] { SpectrumKeyCode.G }},
                { Key.H, new[] { SpectrumKeyCode.H }},
                { Key.J, new[] { SpectrumKeyCode.J }},
                { Key.K, new[] { SpectrumKeyCode.K }},
                { Key.L, new[] { SpectrumKeyCode.L }},
                { Key.Enter, new[] { SpectrumKeyCode.Enter }},

                // --- Fourth ZX Spectrum key row
                { Key.LeftShift, new[] { SpectrumKeyCode.CShift }},
                { Key.RightShift, new[] { SpectrumKeyCode.CShift }},
                { Key.Z, new[] { SpectrumKeyCode.Z }},
                { Key.X, new[] { SpectrumKeyCode.X }},
                { Key.C, new[] { SpectrumKeyCode.C }},
                { Key.V, new[] { SpectrumKeyCode.V }},
                { Key.B, new[] { SpectrumKeyCode.B }},
                { Key.N, new[] { SpectrumKeyCode.N }},
                { Key.M, new[] { SpectrumKeyCode.M }},
                { Key.RightAlt, new[] { SpectrumKeyCode.SShift }},
                { Key.Space, new[] { SpectrumKeyCode.Space }},

                // --- Extra key combinations
                { Key.Back, new[] { SpectrumKeyCode.N0, SpectrumKeyCode.CShift }},
                { Key.Home, new[] { SpectrumKeyCode.N1, SpectrumKeyCode.CShift }},
            };
        }
    }
}
