using Avalonia.Automation.Peers;


namespace Tabalonia.Controls;


public class LeftPressedThumb : TemplatedControl
{
    public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
        RoutedEvent.Register<Thumb, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

    
    private Point? _lastPoint;

    public Point? LastScreenPoint { get; private set; }
    

    public event EventHandler<VectorEventArgs>? DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public event EventHandler<VectorEventArgs>? DragDelta
    {
        add => AddHandler(DragDeltaEvent, value);
        remove => RemoveHandler(DragDeltaEvent, value);
    }

    public event EventHandler<VectorEventArgs>? DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }


    protected override AutomationPeer OnCreateAutomationPeer() => new LeftPressedThumbPeer(this);

    
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector = _lastPoint.Value,
            };

            _lastPoint = null;
            LastScreenPoint = null;

            RaiseEvent(ev);
        }
        
        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!IsLeftButtonPressed(e))
            return;

        LastScreenPoint = GetScreenPoint(e);
        
        if (_lastPoint.HasValue)
        {
            var ev = new VectorEventArgs
            {
                RoutedEvent = DragDeltaEvent,
                Vector = e.GetPosition(this) - _lastPoint.Value,
            };

            RaiseEvent(ev);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsLeftButtonPressed(e))
            return;
        
        e.Handled = true;
        e.Pointer.Capture(this);
        _lastPoint = e.GetPosition(this);
        LastScreenPoint = GetScreenPoint(e);

        var ev = new VectorEventArgs
        {
            RoutedEvent = DragStartedEvent,
            Vector = (Vector)_lastPoint,
        };
        
        e.PreventGestureRecognition();

        RaiseEvent(ev);
    }
    
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            e.Handled = true;
            _lastPoint = null;
            LastScreenPoint = GetScreenPoint(e);
            e.Pointer.Capture(null);

            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector = e.GetPosition(this),
            };

            RaiseEvent(ev);

            LastScreenPoint = null;
        }
    }

    
    private bool IsLeftButtonPressed(PointerEventArgs args)
    {
        var point = args.GetCurrentPoint(this);
        
        return point.Properties.IsLeftButtonPressed;
    }


    private Point? GetScreenPoint(PointerEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
            return null;

        Point pointInTopLevel = e.GetPosition(topLevel);
        PixelPoint screenPoint = topLevel.PointToScreen(pointInTopLevel);

        return new Point(screenPoint.X, screenPoint.Y);
    }
    
    
    private class LeftPressedThumbPeer : ControlAutomationPeer
    {
        public LeftPressedThumbPeer(LeftPressedThumb owner) : base(owner) { }
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Thumb;
        protected override bool IsContentElementCore() => false;
    }
}