using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace SpectrumEngine.Client.Avalonia.Controls.Common;

public partial class VirtualizedList : UserControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsProperty = 
        AvaloniaProperty.Register<VirtualizedList, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<double> ItemHeightProperty = 
        AvaloniaProperty.Register<VirtualizedList, double>(nameof(ItemHeight), double.NaN);

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = 
        AvaloniaProperty.Register<VirtualizedList, IDataTemplate?>(nameof(ItemTemplate));

    public VirtualizedList()
    {
        InitializeComponent();
    }

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set
        {
            SetValue(ItemsProperty, value);
            Scroller.Offset = new Vector(0, -1);
            Scroller.Offset = new Vector(0, 0);
        }
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// The index of the top Item
    /// </summary>
    public int TopIndex { get; private set; } = 0;

    public int VisibleItemsCount { get; private set; } = 0;

    /// <summary>
    /// Number of items visible in the viewport
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnScrollPositionChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;
        TopIndex = ItemHeight == 0 ? 0 : (int)Math.Floor(sv.Offset.Y / ItemHeight);
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;
        VisibleItemsCount = ItemHeight == 0 ? 0 : (int) (sv.Bounds.Height / ItemHeight);
    }
}