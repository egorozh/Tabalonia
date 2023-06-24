using Avalonia.Input;
using Avalonia.Interactivity;
using Tabalonia.Controls;

namespace Tabalonia.Events;

public class DragablzDragDeltaEventArgs : DragablzItemEventArgs
{
    public DragablzDragDeltaEventArgs(DragTabItem dragablzItem, VectorEventArgs dragDeltaEventArgs)
        : base(dragablzItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragablzDragDeltaEventArgs(RoutedEvent routedEvent, DragTabItem dragablzItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, dragablzItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public DragablzDragDeltaEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem dragablzItem, VectorEventArgs dragDeltaEventArgs) 
        : base(routedEvent, source, dragablzItem)
    {
        DragDeltaEventArgs = dragDeltaEventArgs ?? throw new ArgumentNullException(nameof(dragDeltaEventArgs));
    }

    public VectorEventArgs DragDeltaEventArgs { get; }

    public bool Cancel { get; set; }        
}