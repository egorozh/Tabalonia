using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Tabalonia.Events;

namespace Tabalonia.Controls;

public class DragTabItem : TabItem
{
    #region Private Fields

    private Thumb _thumb;
    private int _logicalIndex;
    private bool _isDragging;
    private bool _isSiblingDragging;

    #endregion
    
    #region Internal Properties

    internal Point MouseAtDragStart { get; set; }

    #endregion

    #region Avalonia Properties

    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<DragTabItem, double>(nameof(X));

    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<DragTabItem, double>(nameof(Y));

    public static readonly DirectProperty<DragTabItem, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<DragTabItem, bool>(nameof(IsDragging),
            o => o.IsDragging, (o, v) => o.IsDragging = v);
    
    public static readonly DirectProperty<DragTabItem, int> LogicalIndexProperty =
        AvaloniaProperty.RegisterDirect<DragTabItem, int>(nameof(LogicalIndex),
            o => o.LogicalIndex, (o, v) => o.LogicalIndex = v);

    public static readonly DirectProperty<DragTabItem, bool> IsSiblingDraggingProperty =
        AvaloniaProperty.RegisterDirect<DragTabItem, bool>(nameof(IsSiblingDragging),
            o => o.IsSiblingDragging, (o, v) => o.IsSiblingDragging = v);

    #endregion

    #region Public Properties

    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public int LogicalIndex
    {
        get => _logicalIndex;
        internal set => SetAndRaise(LogicalIndexProperty, ref _logicalIndex, value);
    }
    
    public bool IsDragging
    {
        get => _isDragging;
        internal set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    public bool IsSiblingDragging
    {
        get => _isSiblingDragging;
        internal set => SetAndRaise(IsSiblingDraggingProperty, ref _isSiblingDragging, value);
    }

    #endregion

    #region Routed Events

    public static readonly RoutedEvent<DragablzDragStartedEventArgs> DragStarted =
        RoutedEvent.Register<DragTabItem, DragablzDragStartedEventArgs>("DragStarted", RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragablzDragDeltaEventArgs> DragDelta =
        RoutedEvent.Register<DragTabItem, DragablzDragDeltaEventArgs>("DragDelta", RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragablzDragCompletedEventArgs> DragCompleted =
        RoutedEvent.Register<DragTabItem, DragablzDragCompletedEventArgs>("DragCompleted", RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragablzDragDeltaEventArgs> PreviewDragDelta =
        RoutedEvent.Register<DragTabItem, DragablzDragDeltaEventArgs>("PreviewDragDelta", RoutingStrategies.Tunnel);

    private int _prevZindex;

    #endregion

    #region Events

    #endregion
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var templateThumb = e.Find<Thumb>("PART_Thumb");

        _thumb = templateThumb;
        _thumb.DragStarted += ThumbOnDragStarted;
        _thumb.DragDelta += ThumbOnDragDelta;
        _thumb.DragCompleted += ThumbOnDragCompleted;
    }

    private void ThumbOnDragStarted(object? sender, VectorEventArgs args)
    {
        //MouseAtDragStart = new MouseDevice().GetPosition(this);
        RaiseEvent(new DragablzDragStartedEventArgs(DragStarted, this, args));
    }

    private void ThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        var previewEventArgs = new DragablzDragDeltaEventArgs(PreviewDragDelta, this, e);
        RaiseEvent(previewEventArgs);
        //if (previewEventArgs.Cancel)
        //    thumb.CancelDrag();
        if (!previewEventArgs.Handled)
        {
            var eventArgs = new DragablzDragDeltaEventArgs(DragDelta, this, e);
            RaiseEvent(eventArgs);
            //if (eventArgs.Cancel)
            //    thumb.CancelDrag();
        }
    }

    private void ThumbOnDragCompleted(object? sender, VectorEventArgs e)
    {
       
        var args = new DragablzDragCompletedEventArgs(DragCompleted, this, e);
        RaiseEvent(args);
        MouseAtDragStart = new Point();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        _prevZindex = ZIndex;
        ZIndex = int.MaxValue;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        ZIndex = _prevZindex;
    }
}