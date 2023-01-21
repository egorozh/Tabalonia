﻿using Avalonia.Input;
using Avalonia.Interactivity;
using TabaloniaNew.Controls;


namespace TabaloniaNew.Events;


public class DragTabDragCompletedEventArgs : DragTabItemEventArgs
{
    public DragTabDragCompletedEventArgs(DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs) 
        :base(dragItem)
    {
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, dragItem)
    {
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }

    public DragTabDragCompletedEventArgs(RoutedEvent routedEvent, IInteractive source, DragTabItem dragItem, VectorEventArgs dragCompletedEventArgs)
        : base(routedEvent, source, dragItem)
    {
        
        //DragCompletedEventArgs = dragCompletedEventArgs ?? throw new ArgumentNullException(nameof(dragCompletedEventArgs));
    }


    //public VectorEventArgs DragCompletedEventArgs { get; }
}