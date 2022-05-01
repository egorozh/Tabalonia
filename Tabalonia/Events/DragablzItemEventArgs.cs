using Avalonia.Interactivity;

namespace Tabalonia.Events;

public class DragablzItemEventArgs : RoutedEventArgs
{
    public DragablzItemEventArgs(DragTabItem dragablzItem)
    {
        DragablzItem = dragablzItem ?? throw new ArgumentNullException(nameof(dragablzItem));
    }

    public DragablzItemEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem)
        : base(routedEvent)
    {
        DragablzItem = dragablzItem;
    }

    public DragablzItemEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem dragablzItem)
        : base(routedEvent, source) 
    {
        DragablzItem = dragablzItem;
    }

    public DragTabItem DragablzItem { get; }
}