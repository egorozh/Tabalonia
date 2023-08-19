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

            RaiseEvent(ev);
        }
        
        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!IsLeftButtonPressed(e))
            return;
        
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
        _lastPoint = e.GetPosition(this);

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
        if (!IsLeftButtonPressed(e))
            return;
        
        if (_lastPoint.HasValue)
        {
            e.Handled = true;
            _lastPoint = null;

            var ev = new VectorEventArgs
            {
                RoutedEvent = DragCompletedEvent,
                Vector = e.GetPosition(this),
            };

            RaiseEvent(ev);
        }
    }

    
    private bool IsLeftButtonPressed(PointerEventArgs args)
    {
        var point = args.GetCurrentPoint(this);
        
        return point.Properties.IsLeftButtonPressed;
    }
    
    
    private class LeftPressedThumbPeer : ControlAutomationPeer
    {
        public LeftPressedThumbPeer(LeftPressedThumb owner) : base(owner) { }
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Thumb;
        protected override bool IsContentElementCore() => false;
    }
}