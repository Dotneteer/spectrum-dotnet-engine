using Material.Icons;
using System.Collections.Generic;

namespace SpectrumEngine.Client.Avalonia.Models
{
    public record MenuItemModel(
        int Id,
        MaterialIconKind IconKind,
        string Text,
        IEnumerable<MenuItemModel>? SubItems = null);
}
