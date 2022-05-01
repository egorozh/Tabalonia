using Avalonia.Input;
using Avalonia.Interactivity;

namespace Tabalonia.Events;

public class DragablzDragDeltaEventArgs : DragablzItemEventArgs
{
    public DragablzDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, dragablzItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public VectorEventArgs DragDeltaEventArgs { get; }

    public bool Cancel { get; set; }        
}