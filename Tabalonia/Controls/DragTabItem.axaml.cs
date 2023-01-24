using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Tabalonia.Events;

namespace Tabalonia.Controls;

public class DragTabItem : TabItem
{
    #region Private Fields

    private CustomThumb? _thumb;
    private int _logicalIndex;
    private bool _isDragging;
    private bool _isSiblingDragging;
    private int _prevZindex;

    private Action<DragTabItem>? _dragSeizedContinuation;
    private bool _seizeDragWithTemplate;

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
        RoutedEvent.Register<DragTabItem, DragablzDragDeltaEventArgs>("PreviewDragDelta",
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

    private PointerEventArgs _pointerEventArgs;

    #endregion

    #region Events

    #endregion

    #region Internal Methods

    internal void InstigateDragPrepare(Action<DragTabItem> continuation, PointerEventArgs pointerEventArgs)
    {
        _dragSeizedContinuation = continuation;
        _pointerEventArgs = pointerEventArgs;
        //if (GetTemplateChild(ThumbPartName) is Thumb thumb)
        //{
        //    thumb.CaptureMouse();
        //}
        //else

        //pointerEventArgs.Pointer.Capture(_thumb);

        _seizeDragWithTemplate = true;
        
        //var cursor = this.Cursor?.PlatformImpl;
        ////Mouse.AddLostMouseCaptureHandler(this, LostMouseAfterSeizeHandler);


        //Dispatcher.BeginInvoke(new Action(() => thumbAndSubscription.Item1.RaiseEvent(new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice,
        //        0,
        //        MouseButton.Left)
        //    { RoutedEvent = MouseLeftButtonDownEvent })));
    }

    internal void InstigateDrag()
    {
        if (_seizeDragWithTemplate)
        {
            //_isTemplateThumbWithMouseAfterSeize = true;
            //Mouse.AddLostMouseCaptureHandler(this, LostMouseAfterSeizeHandler);

            _dragSeizedContinuation?.Invoke(this);
            _dragSeizedContinuation = null;

            RoutedEventArgs args = new PointerPressedEventArgs(_thumb, _pointerEventArgs.Pointer, _thumb,
                new Point(), 0,
                PointerPointProperties.None, KeyModifiers.None);

            Dispatcher.UIThread.Post(() => _thumb?.RaiseEvent(args));

            _seizeDragWithTemplate = false;
        }
    }

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var templateThumb = e.Find<CustomThumb>("PART_Thumb");

        _thumb = templateThumb;

        _thumb.DragStarted += ThumbOnDragStarted;
        _thumb.DragDelta += ThumbOnDragDelta;
        _thumb.DragCompleted += ThumbOnDragCompleted;
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

    protected override void OnPointerEnter(PointerEventArgs e)
    {
        base.OnPointerEnter(e);

        _prevZindex = ZIndex;
        ZIndex = int.MaxValue;
    }

    protected override void OnPointerLeave(PointerEventArgs e)
    {
        base.OnPointerLeave(e);

        ZIndex = _prevZindex;
    }

    #endregion

    #region Private Methods

    private void ThumbOnDragStarted(object? sender, CustomThumbEventArgs args)
    {
        MouseAtDragStart = new MouseDevice().GetPosition(this);
        RaiseEvent(new DragablzDragStartedEventArgs(DragStarted, this, args));
    }

    private void ThumbOnDragDelta(object? sender, CustomThumbEventArgs e)
    {
        if (sender is not CustomThumb thumb)
            return;

        var previewEventArgs = new DragablzDragDeltaEventArgs(PreviewDragDelta, this, e);
        RaiseEvent(previewEventArgs);

        if (previewEventArgs.Cancel)
            CancelDrag(thumb, e.Vector, e.PointerEventArgs);

        if (!previewEventArgs.Handled)
        {
            var eventArgs = new DragablzDragDeltaEventArgs(DragDelta, this, e);
            RaiseEvent(eventArgs);

            if (eventArgs.Cancel)
                CancelDrag(thumb, e.Vector, e.PointerEventArgs);
        }
    }

    private void ThumbOnDragCompleted(object? sender, CustomThumbEventArgs e)
    {
        var args = new DragablzDragCompletedEventArgs(DragCompleted, this);
        RaiseEvent(args);
        MouseAtDragStart = new Point();
    }

    private static void CancelDrag(CustomThumb thumb, Vector vector, PointerEventArgs pointerEventArgs)
    {
        CustomThumbEventArgs ev = new(pointerEventArgs)
        {
            RoutedEvent = CustomThumb.DragCompletedEvent,
            Vector = vector,
        };

        thumb.RaiseEvent(ev);
    }

    #endregion
}