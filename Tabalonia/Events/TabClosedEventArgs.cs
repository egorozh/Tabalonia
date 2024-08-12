namespace Tabalonia.Events;

public class TabClosedEventArgs(object? item) : EventArgs
{
    public object? Item { get; } = item;
}
