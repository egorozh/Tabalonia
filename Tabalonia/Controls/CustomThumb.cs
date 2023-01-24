using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Tabalonia.Events;

namespace Tabalonia.Controls;

[PseudoClasses(":pressed")]
public class CustomThumb : TemplatedControl
{
    public static readonly RoutedEvent<CustomThumbEventArgs> DragStartedEvent =
        RoutedEvent.Register<CustomThumb, CustomThumbEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<CustomThumbEventArgs> DragDeltaEvent =
        RoutedEvent.Register<CustomThumb, CustomThumbEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<CustomThumbEventArgs> DragCompletedEvent =
        RoutedEvent.Register<CustomThumb, CustomThumbEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

    private Point? _lastPoint;

    static CustomThumb()
    {
        DragStartedEvent.AddClassHandler<CustomThumb>((x, e) => x.OnDragStarted(e), RoutingStrategies.Bubble);
        DragDeltaEvent.AddClassHandler<CustomThumb>((x, e) => x.OnDragDelta(e), RoutingStrategies.Bubble);
        DragCompletedEvent.AddClassHandler<CustomThumb>((x, e) => x.OnDragCompleted(e), RoutingStrategies.Bubble);
    }

    public event EventHandler<CustomThumbEventArgs> DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public event EventHandler<CustomThumbEventArgs> DragDelta
    {
        add => AddHandler(DragDeltaEvent, value);
        remove => RemoveHandler(DragDeltaEvent, value);
    }

    public event EventHandler<CustomThumbEventArgs> DragCompleted
    {
        add => AddHandler(DragCompletedEvent, value);
        remove => RemoveHandler(DragCompletedEvent, value);
    }

    protected virtual void OnDragStarted(CustomThumbEventArgs e)
    {
    }

    protected virtual void OnDragDelta(CustomThumbEventArgs e)
    {
    }

    protected virtual void OnDragCompleted(CustomThumbEventArgs e)
    {
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            var ev = new CustomThumbEventArgs(e)
            {
                RoutedEvent = DragCompletedEvent,
                Vector = _lastPoint.Value,
            };

            _lastPoint = null;

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(":pressed");

        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            var ev = new CustomThumbEventArgs(e)
            {
                RoutedEvent = DragDeltaEvent,
                Vector = e.GetPosition(this) - _lastPoint.Value
            };

            RaiseEvent(ev);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
        _lastPoint = e.GetPosition(this);

        var ev = new CustomThumbEventArgs(e)
        {
            RoutedEvent = DragStartedEvent,
            Vector = (Vector)_lastPoint,
        };

        PseudoClasses.Add(":pressed");

        RaiseEvent(ev);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_lastPoint.HasValue)
        {
            e.Handled = true;
            _lastPoint = null;

            var ev = new CustomThumbEventArgs(e)
            {
                RoutedEvent = DragCompletedEvent,
                Vector = e.GetPosition(this),
            };

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(":pressed");
    }
}