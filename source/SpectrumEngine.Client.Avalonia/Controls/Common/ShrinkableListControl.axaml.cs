using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;

namespace SpectrumEngine.Client.Avalonia.Controls.Common;

public class ShrinkableListControl : TemplatedControl
{
    private List<object> _items = new();
    private ObservableCollection<object> _displayedItems = new();
    private bool _overflow;
    private bool _itemSizeObtained;
    private double _itemSize;

    public const int GAP = 80;
    
    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<ShrinkableListControl, List<object>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<ShrinkableListControl, List<object>>(
            nameof(Items), 
            o => o.Items, 
            (o, v) => o.Items = v);

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<ShrinkableListControl, ObservableCollection<object>> DisplayedItemsProperty =
        AvaloniaProperty.RegisterDirect<ShrinkableListControl, ObservableCollection<object>>(
            nameof(DisplayedItems), 
            o => o.DisplayedItems);

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<ShrinkableListControl, bool> OverflowProperty =
        AvaloniaProperty.RegisterDirect<ShrinkableListControl, bool>(
            nameof(Overflow), 
            o => o.Overflow);

    /// <summary>
    /// Defines the <see cref="ItemTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        ItemsControl.ItemTemplateProperty.AddOwner<ShrinkableListControl>();

    public ShrinkableListControl()
    {
        var items = new List<object>();
        for (var i = 0; i < 40; i++)
        {
            items.Add($"Item {i}");
        }
        Items = items;
    }
    
    /// <summary>
    /// Gets or sets the items to display.
    /// </summary>
    public List<object> Items
    {
        get => _items;
        set
        {
            SetAndRaise(ItemsProperty, ref _items, value);
            DisplayedItems = new ObservableCollection<object>(_items);
            _itemSizeObtained = false;
        }
    }

    /// <summary>
    /// Gets or sets the items to display.
    /// </summary>
    public ObservableCollection<object> DisplayedItems
    {
        get => _displayedItems;
        private set => SetAndRaise(DisplayedItemsProperty, ref _displayedItems, value);
    }

    /// <summary>
    /// Indicates if the items overflow their container
    /// </summary>
    public bool Overflow
    {
        get => _overflow;
        private set => SetAndRaise(OverflowProperty, ref _overflow, value);
    }
    
    /// <summary>
    /// Gets or sets the data template used to display the items in the control.
    /// </summary>
    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
        {
            AdjustDisplayedItems();
        }
    }

    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();
        AdjustDisplayedItems();
    }

    private void AdjustDisplayedItems()
    {
        // --- Obtain the size of the first item (we assume, all item has the same height
        if (!_itemSizeObtained)
        {
            var grid = this.GetVisualChildren().ToList()[0];
            var stackPanel = grid.GetVisualChildren().ToList()[0];
            var itemsRepeater = stackPanel.GetVisualChildren().ToList()[0];
            var listItems = itemsRepeater.GetVisualChildren().ToList();
            if (listItems.Count > 0)
            {
                _itemSize = listItems[0].Bounds.Height;
                _itemSizeObtained = true;
            }
        }
        
        // --- Determine the number of visible items
        if (!_itemSizeObtained) return;

        // --- Try without the GAP
        var maxHeight = this.GetVisualChildren().ToList()[0].GetVisualChildren().ToList()[0].Bounds.Height;
        var itemCount = Math.Floor(maxHeight / _itemSize);
        Overflow = itemCount < Items.Count;

        if (DisplayedItems.Count < itemCount)
        {
            for (var i = DisplayedItems.Count; i < Items.Count; i++)
            {
                DisplayedItems.Add(Items[i]);
            }
        }        
        else if (DisplayedItems.Count > itemCount && DisplayedItems.Count > 0)
        {
            for (var i = DisplayedItems.Count - 1; i >= itemCount; i--)
            {
                DisplayedItems.RemoveAt(i);
            }
        }
    }
}