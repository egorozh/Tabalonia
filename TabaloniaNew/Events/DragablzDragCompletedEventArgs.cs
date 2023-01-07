using Avalonia.Input;
using Avalonia.Interactivity;
using TabaloniaNew.Controls;

namespace TabaloniaNew.Events;

public class DragablzDragCompletedEventArgs : RoutedEventArgs
{
    public DragablzDragCompletedEventArgs(DragTabItem dragablzItem, VectorEventArgs dragCompletedEventArgs)
    {
        DragablzItem = dragablzItem ?? throw new ArgumentNullException(nameof(dragablzItem));
        DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragablzDragCompletedEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent)
    {
        DragablzItem = dragablzItem ?? throw new ArgumentNullException(nameof(dragablzItem));            
        DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragablzDragCompletedEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem dragablzItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, source)
    {
        DragablzItem = dragablzItem ?? throw new ArgumentNullException(nameof(dragablzItem));
        DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabItem DragablzItem { get; }

    public VectorEventArgs DragCompletedEventArgs { get; }
}