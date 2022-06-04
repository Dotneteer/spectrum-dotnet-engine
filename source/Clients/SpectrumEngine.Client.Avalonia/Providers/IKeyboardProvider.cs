using Avalonia.Input;
using SpectrumEngine.Emu;
using System.Collections.Generic;

namespace SpectrumEngine.Client.Avalonia.Providers
{
    /// <summary>
    /// Interface for declare emulator keyboard. 
    /// All emulators keyboards should be inherited from this interface for implement custom keyboard mappings.
    /// </summary>
    public interface IKeyboardProvider
    {
        /// <summary>
        /// Get keyboard type
        /// </summary>
        Keyboard Keyboard { get; }

        /// <summary>
        /// Get emulator machine keys from keyboard key
        /// </summary>
        /// <param name="key">Keyboard key</param>
        /// <returns>Emulator machine key, null if not found</returns>
        IEnumerable<SpectrumKeyCode> MapKey(Key key);
    }
}
