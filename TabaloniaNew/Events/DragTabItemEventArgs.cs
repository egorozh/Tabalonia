using Avalonia.Interactivity;
using TabaloniaNew.Controls;

namespace TabaloniaNew.Events;


public abstract class DragTabItemEventArgs : RoutedEventArgs
{
    protected DragTabItemEventArgs(DragTabItem dragTabItem)
    {
        TabItem = dragTabItem ?? throw new ArgumentNullException(nameof(dragTabItem));
    }

    protected DragTabItemEventArgs(RoutedEvent routedEvent, DragTabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }

    protected DragTabItemEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem tabItem)
        : base(routedEvent, source) 
    {
        TabItem = tabItem;
    }

    
    
    public DragTabItem TabItem { get; }
}