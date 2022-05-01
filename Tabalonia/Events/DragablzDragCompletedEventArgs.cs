using Avalonia.Interactivity;

namespace Tabalonia.Events;

public sealed class DragablzDragCompletedEventArgs : DragTabItemEventArgs
{
    public DragablzDragCompletedEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem)
        : base(routedEvent, dragablzItem)
    {
    }
}