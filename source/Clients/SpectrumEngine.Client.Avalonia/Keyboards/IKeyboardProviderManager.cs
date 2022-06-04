using Avalonia.Input;
using SpectrumEngine.Emu;
using System.Collections.Generic;

namespace SpectrumEngine.Client.Avalonia.Keyboards
{
    /// <summary>
    /// Interface for manage emulator keyboards
    /// </summary>
    public interface IKeyboardProviderManager
    {
        /// <summary>
        /// Get or set emulator keyboard to use
        /// </summary>
        Keyboard Keyboard { get; set; }

        /// <summary>
        /// Get emulator machine keys from keyboard key
        /// </summary>
        /// <param name="key">Keyboard key</param>
        /// <returns>Emulator machine key, null if not found</returns>
        IEnumerable<SpectrumKeyCode> MapKey(Key key);
    }
}