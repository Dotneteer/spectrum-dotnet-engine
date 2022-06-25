using Avalonia;
using Avalonia.Controls.Primitives;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class SplitterPanel : Thumb
{
    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        if (change.Property.Name == "Orientation")
        {
            // --- Orientation changed
        }
        base.OnPropertyChanged(change);
    }
}