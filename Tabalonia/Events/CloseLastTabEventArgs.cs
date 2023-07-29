namespace Tabalonia.Events;


public class CloseLastTabEventArgs : EventArgs
{
    public Window? Window { get; }

    
    public CloseLastTabEventArgs(Window? window)
    {
        Window = window;
    }
}