using Avalonia.Interactivity;

namespace Tabalonia.Events;

public sealed class DragablzDragDeltaEventArgs : DragTabItemEventArgs
{
    public CustomThumbEventArgs DragDeltaEventArgs { get; }

    public bool Cancel { get; set; }

    public DragablzDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem, CustomThumbEventArgs dragDeltaEventArgs)       
        : base(routedEvent, dragablzItem)
    {
        ArgumentNullException.ThrowIfNull(dragDeltaEventArgs, nameof(dragDeltaEventArgs));

        DragDeltaEventArgs = dragDeltaEventArgs;
    }
}