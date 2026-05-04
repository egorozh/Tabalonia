using Tabalonia.Controls;

namespace Tabalonia.Events;


public class DragTabDragCompletedEventArgs : DragTabItemEventArgs
{
    public DragTabDragCompletedEventArgs(DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs) 
        :base(dragItem)
    {
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs, Point? screenPoint)
        : this(dragItem, dragCompletedEventArgs)
    {
        ScreenPoint = screenPoint;
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, dragItem)
    {
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs, Point? screenPoint)
        : this(routedEvent, dragItem, dragCompletedEventArgs)
    {
        ScreenPoint = screenPoint;
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, Interactive source, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, source, dragItem)
    {
        
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(
        RoutedEvent routedEvent,
        Interactive source,
        DragTabItem dragItem,
        VectorEventArgs dragCompletedEventArgs,
        Point? screenPoint)
        : this(routedEvent, source, dragItem, dragCompletedEventArgs)
    {
        ScreenPoint = screenPoint;
    }


    //public VectorEventArgs DragCompletedEventArgs { get; }

    public Point? ScreenPoint { get; }
}