using Avalonia.Interactivity;

namespace Tabalonia.Events;

public abstract class DragTabItemEventArgs : RoutedEventArgs
{
    public DragTabItem DragablzItem { get; }

    protected DragTabItemEventArgs(RoutedEvent routedEvent, DragTabItem dragTabItem) : base(routedEvent)
    {
        ArgumentNullException.ThrowIfNull(dragTabItem, nameof(dragTabItem));

        DragablzItem = dragTabItem;
    }
}