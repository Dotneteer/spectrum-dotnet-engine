using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace SpectrumEngine.Client.Avalonia.Controls;

public class SplitterPanel : Thumb
{
    private const int KEY_RESIZE = 10;
    private ILogical? _parent; 
    private int _splitterIndex;
    private double _startSize;
    
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        AvaloniaProperty.Register<SplitterPanel, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<bool> ResizePreviousProperty = 
        AvaloniaProperty.Register<SplitterPanel, bool>(nameof(ResizePrevious), true);

    public static readonly StyledProperty<double> PreviousMinSizeProperty = 
        AvaloniaProperty.Register<SplitterPanel, double>(nameof(PreviousMinSize), double.NaN);

    public static readonly StyledProperty<bool> ResizeNextProperty = 
        AvaloniaProperty.Register<SplitterPanel, bool>(nameof(ResizeNext), true);

    public static readonly StyledProperty<double> NextMinSizeProperty = 
        AvaloniaProperty.Register<SplitterPanel, double>(nameof(NextMinSize), double.NaN);

    public static readonly StyledProperty<bool> NegateDeltaProperty = 
        AvaloniaProperty.Register<SplitterPanel, bool>(nameof(NegateDelta));

    public SplitterPanel()
    {
        Orientation = Orientation.Horizontal;
        Cursor = new Cursor(StandardCursorType.SizeWestEast);
    }
    
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool ResizePrevious
    {
        get => GetValue(ResizePreviousProperty);
        set => SetValue(ResizePreviousProperty, value);
    }

    public double PreviousMinSize
    {
        get => GetValue(PreviousMinSizeProperty);
        set => SetValue(PreviousMinSizeProperty, value);
    }
    
    public bool ResizeNext
    {
        get => GetValue(ResizeNextProperty);
        set => SetValue(ResizeNextProperty, value);
    }
    
    public double NextMinSize
    {
        get => GetValue(NextMinSizeProperty);
        set => SetValue(NextMinSizeProperty, value);
    }
    
    public bool NegateDelta
    {
        get => GetValue(NegateDeltaProperty);
        set => SetValue(NegateDeltaProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                if (Orientation == Orientation.Horizontal) Resize(-KEY_RESIZE, _startSize);
                e.Handled = true;
                break;
            case Key.Right:
                if (Orientation == Orientation.Horizontal) Resize(KEY_RESIZE, _startSize);
                e.Handled = true;
                break;
            case Key.Up:
                if (Orientation == Orientation.Vertical) Resize(-KEY_RESIZE, _startSize);
                e.Handled = true;
                break;
            case Key.Down:
                if (Orientation == Orientation.Vertical) Resize(KEY_RESIZE, _startSize);
                e.Handled = true;
                break;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        if (change.Property.Name == "Orientation")
        {
            // --- Orientation changed
            Cursor = new Cursor(change.NewValue is Orientation.Horizontal 
                ? StandardCursorType.SizeWestEast 
                : StandardCursorType.SizeNorthSouth);
        }
        base.OnPropertyChanged(change);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _parent = this.GetLogicalParent();
        if (_parent == null) return;
        _splitterIndex = _parent.GetLogicalChildren().Count() - 1 ;
    }

    protected override void OnDragStarted(VectorEventArgs e)
    {
        base.OnDragStarted(e);
        _startSize = Orientation == Orientation.Horizontal ? e.Vector.X : e.Vector.Y;
    }

    protected override void OnDragCompleted(VectorEventArgs e)
    {
        base.OnDragCompleted(e);
        Resize(Orientation == Orientation.Horizontal ? e.Vector.X : e.Vector.Y, _startSize);
    }

    protected override void OnDragDelta(VectorEventArgs e)
    {
        base.OnDragDelta(e);
        Resize(Orientation == Orientation.Horizontal ? e.Vector.X : e.Vector.Y, 0.0);
    }

    private void Resize(double delta, double offset)
    {
        // --- Check the logical tree for previous and next siblings
        if (_parent == null || _splitterIndex < 1) return;
        var children = _parent.GetLogicalChildren().ToList();
        if (_splitterIndex >= children.Count - 1) return;

        // --- Now, we can get the siblings to resize
        if (children[_splitterIndex - 1] is not Control previousControl
            || children[_splitterIndex + 1] is not Control nextControl) return;

        var prevSize = 
            (Orientation == Orientation.Horizontal 
                ? previousControl.Bounds.Width 
                : previousControl.Bounds.Height) - 
            (delta - offset) * (NegateDelta ? -1.0 : 1.0); 
        if (!double.IsNaN(PreviousMinSize) && prevSize < PreviousMinSize)
        {
            delta -= PreviousMinSize - prevSize;
            prevSize = PreviousMinSize;
        }
        var nextSize = 
            (Orientation == Orientation.Horizontal 
                ? nextControl.Bounds.Width 
                : nextControl.Bounds.Height) + 
            (delta - offset) * (NegateDelta ? -1.0 : 1.0);
        if (!double.IsNaN(NextMinSize) && nextSize < NextMinSize)
        {
            prevSize += nextSize - NextMinSize;
            nextSize = NextMinSize;
        }
        
        if (ResizePrevious)
        {
            if (Orientation == Orientation.Horizontal)
            {
                previousControl.Width = prevSize;
            }
            else
            {
                previousControl.Height = prevSize;
            }
        }

        if (!ResizeNext) return;
        
        if (Orientation == Orientation.Horizontal)
        {
            nextControl.Width = nextSize;
        }
        else
        {
            nextControl.Height = nextSize;
        }
    }
}