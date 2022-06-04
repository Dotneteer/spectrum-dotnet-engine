using Avalonia.Input;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Emu;
using Splat;
using System.Collections.Generic;

namespace SpectrumEngine.Client.Avalonia.Providers
{
    public class KeyboardProviderManager : IKeyboardProviderManager
    {
        private IKeyboardProvider currentKeyboard;

        public KeyboardProviderManager(IKeyboardProvider? defaultKeyboard = null)
        {
            currentKeyboard = defaultKeyboard ?? CreateKeyboard(Keyboard.ZxSpectrum48);
        }

        public Keyboard Keyboard
        {
            get => currentKeyboard.Keyboard;
            set
            {
                if (currentKeyboard.Keyboard != value) currentKeyboard = CreateKeyboard(value);
            }
        }

        public IEnumerable<SpectrumKeyCode> MapKey(Key key) => currentKeyboard.MapKey(key);

        private static IKeyboardProvider CreateKeyboard(Keyboard keyboard) => Locator.Current.GetRequiredService<IKeyboardProvider>(keyboard.ToString());
    }
}
