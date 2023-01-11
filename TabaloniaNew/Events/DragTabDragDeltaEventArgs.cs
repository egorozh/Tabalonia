using Avalonia.Input;
using Avalonia.Interactivity;
using TabaloniaNew.Controls;


namespace TabaloniaNew.Events;


public class DragTabDragDeltaEventArgs : DragTabItemEventArgs
{
    public DragTabDragDeltaEventArgs(DragTabItem dragTabItem, VectorEventArgs dragDeltaEventArgs)
        : base(dragTabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, tabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragTabDragDeltaEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem tabItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, source, tabItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public VectorEventArgs DragDeltaEventArgs { get; }

    public bool Cancel { get; set; }        
}