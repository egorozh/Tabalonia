using Avalonia.Input;
using Avalonia.Interactivity;
using Tabalonia.Controls;

namespace Tabalonia.Events;

public class DragablzDragStartedEventArgs : DragablzItemEventArgs
{
    public DragablzDragStartedEventArgs(DragTabItem dragablzItem, VectorEventArgs dragStartedEventArgs)
        : base(dragablzItem)
    {
        DragStartedEventArgs = dragStartedEventArgs ?? throw new ArgumentNullException(nameof(dragStartedEventArgs));
    }

    public DragablzDragStartedEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem, VectorEventArgs dragStartedEventArgs)
        : base(routedEvent, dragablzItem)
    {
        DragStartedEventArgs = dragStartedEventArgs;
    }

    public DragablzDragStartedEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem dragablzItem, VectorEventArgs dragStartedEventArgs)
        : base(routedEvent, source, dragablzItem)
    {
        DragStartedEventArgs = dragStartedEventArgs;
    }

    public VectorEventArgs DragStartedEventArgs { get; }
}