using Tabalonia.Controls;

namespace Tabalonia.Events;


public class DragTabDragDeltaEventArgs : DragTabItemEventArgs
{
    public DragTabDragDeltaEventArgs(DragTabItem dragTabItem, VectorEventArgs dragDeltaEventArgs)
        : base(dragTabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(DragTabItem dragTabItem, VectorEventArgs dragDeltaEventArgs, Point? screenPoint)
        : this(dragTabItem, dragDeltaEventArgs)
    {
        ScreenPoint = screenPoint;
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, tabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs, Point? screenPoint)
        : this(routedEvent, tabItem, dragDeltaEventArgs)
    {
        ScreenPoint = screenPoint;
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, Interactive source, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, source, tabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(
        RoutedEvent routedEvent,
        Interactive source,
        DragTabItem tabItem,
        VectorEventArgs dragDeltaEventArgs,
        Point? screenPoint)
        : this(routedEvent, source, tabItem, dragDeltaEventArgs)
    {
        ScreenPoint = screenPoint;
    }

    public VectorEventArgs DragDeltaEventArgs { get; }

    public Point? ScreenPoint { get; }

    public bool Cancel { get; set; }        
}