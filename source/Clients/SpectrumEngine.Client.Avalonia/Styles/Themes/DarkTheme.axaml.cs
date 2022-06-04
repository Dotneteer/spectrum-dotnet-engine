using Avalonia.Markup.Xaml;
using AvaloniaStyles = Avalonia.Styling.Styles;

namespace SpectrumEngine.Client.Avalonia.Styles;

public class DarkTheme : AvaloniaStyles
{
    public DarkTheme() => AvaloniaXamlLoader.Load(this);
}