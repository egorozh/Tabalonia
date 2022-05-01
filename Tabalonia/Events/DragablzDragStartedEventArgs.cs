using Avalonia.Interactivity;

namespace Tabalonia.Events;

public sealed class DragablzDragStartedEventArgs : DragTabItemEventArgs
{
    public CustomThumbEventArgs DragStartedEventArgs { get; }

    public DragablzDragStartedEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem, CustomThumbEventArgs dragStartedEventArgs) 
        : base(routedEvent, dragablzItem)
    {
        ArgumentNullException.ThrowIfNull(dragStartedEventArgs, nameof(dragStartedEventArgs));

        DragStartedEventArgs = dragStartedEventArgs;
    }
}