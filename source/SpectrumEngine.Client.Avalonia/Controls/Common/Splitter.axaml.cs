using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
// ReSharper disable MemberCanBePrivate.Global

namespace SpectrumEngine.Client.Avalonia.Controls;

/// <summary>
/// This control implements a splitter panel that can be used to resize child panels
/// </summary>
public class Splitter : Thumb
{
    // --- Sizingh unit for a single keypress
    private const int KEY_RESIZE = 10;
    
    // --- Parent control
    private ILogical? _parent;
    
    // --- Index of the splitter within its parent
    private int _splitterIndex;
    private double _startPosition;
    
    public static readonly StyledProperty<Orientation> OrientationProperty = 
        AvaloniaProperty.Register<Splitter, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<bool> ResizePreviousProperty = 
        AvaloniaProperty.Register<Splitter, bool>(nameof(ResizePrevious), true);

    public static readonly StyledProperty<double> PreviousMinSizeProperty = 
        AvaloniaProperty.Register<Splitter, double>(nameof(PreviousMinSize), double.NaN);

    public static readonly StyledProperty<bool> ResizeNextProperty = 
        AvaloniaProperty.Register<Splitter, bool>(nameof(ResizeNext), true);

    public static readonly StyledProperty<double> NextMinSizeProperty = 
        AvaloniaProperty.Register<Splitter, double>(nameof(NextMinSize), double.NaN);

    public static readonly StyledProperty<bool> NegateDeltaProperty = 
        AvaloniaProperty.Register<Splitter, bool>(nameof(NegateDelta));

    public Splitter()
    {
        Orientation = Orientation.Horizontal;
        Cursor = new Cursor(StandardCursorType.SizeWestEast);
    }
    
    /// <summary>
    /// The splitter's orientation
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Indicates if the control before the splitter should be resized
    /// </summary>
    public bool ResizePrevious
    {
        get => GetValue(ResizePreviousProperty);
        set => SetValue(ResizePreviousProperty, value);
    }

    /// <summary>
    /// The minimum size of the control before the splitter
    /// </summary>
    public double PreviousMinSize
    {
        get => GetValue(PreviousMinSizeProperty);
        set => SetValue(PreviousMinSizeProperty, value);
    }
    
    /// <summary>
    /// Indicates if the control after the splitter should be resized
    /// </summary>
    public bool ResizeNext
    {
        get => GetValue(ResizeNextProperty);
        set => SetValue(ResizeNextProperty, value);
    }
    
    /// <summary>
    /// The minimum size of the control after the splitter
    /// </summary>
    public double NextMinSize
    {
        get => GetValue(NextMinSizeProperty);
        set => SetValue(NextMinSizeProperty, value);
    }
    
    /// <summary>
    /// Indicates if the current movement (delta) should be negated
    /// </summary>
    public bool NegateDelta
    {
        get => GetValue(NegateDeltaProperty);
        set => SetValue(NegateDeltaProperty, value);
    }

    /// <summary>
    /// Allow keypressed to move the splitter
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                if (Orientation == Orientation.Horizontal) Resize(-KEY_RESIZE, _startPosition);
                e.Handled = true;
                break;
            case Key.Right:
                if (Orientation == Orientation.Horizontal) Resize(KEY_RESIZE, _startPosition);
                e.Handled = true;
                break;
            case Key.Up:
                if (Orientation == Orientation.Vertical) Resize(-KEY_RESIZE, _startPosition);
                e.Handled = true;
                break;
            case Key.Down:
                if (Orientation == Orientation.Vertical) Resize(KEY_RESIZE, _startPosition);
                e.Handled = true;
                break;
        }
        base.OnKeyDown(e);
    }

    /// <summary>
    /// Updates the splitter cursor when the orientation changes
    /// </summary>
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

    /// <summary>
    /// Finds the splitter panel's index in its parent's child controls collection
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _parent = this.GetLogicalParent();
        if (_parent == null) return;
        var children = _parent.GetLogicalChildren().ToArray();
        for (var i = 0; i < children.Length; i++)
        {
            if (children[i] is Splitter)
            {
                _splitterIndex = i;
            }
        }
    }

    /// <summary>
    /// Store the start position when dragging starts
    /// </summary>
    protected override void OnDragStarted(VectorEventArgs e)
    {
        base.OnDragStarted(e);
        _startPosition = Orientation == Orientation.Horizontal ? e.Vector.X : e.Vector.Y;
    }

    /// <summary>
    /// Complete moving the splitter when dragging completes
    /// </summary>
    protected override void OnDragCompleted(VectorEventArgs e)
    {
        base.OnDragCompleted(e);
        Resize(Orientation == Orientation.Horizontal ? e.Vector.X : e.Vector.Y, _startPosition);
    }

    /// <summary>
    /// Move the splitter when pointer is moving 
    /// </summary>
    protected override void OnDragDelta(VectorEventArgs e)
    {
        base.OnDragDelta(e);
        Resize(Orientation == Orientation.Horizontal ? e.Vector.X : e.Vector.Y, 0.0);
    }

    /// <summary>
    /// Implement the resize logic
    /// </summary>
    /// <param name="delta">Movement</param>
    /// <param name="offset">Offset to use</param>
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