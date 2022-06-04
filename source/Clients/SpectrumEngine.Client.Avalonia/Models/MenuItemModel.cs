using Material.Icons;
using System.Collections.Generic;

namespace SpectrumEngine.Client.Avalonia.Models
{
    /// <summary>
    /// Menu Item model
    /// </summary>
    /// <param name="Id">Id for menu item</param>
    /// <param name="IconKind">Icon to show in menu item</param>
    /// <param name="Text">Text to show in menu item</param>
    /// <param name="SubItems">subitems</param>
    public record MenuItemModel(
        int Id,
        MaterialIconKind IconKind,
        string Text,
        IEnumerable<MenuItemModel>? SubItems = null);
}
