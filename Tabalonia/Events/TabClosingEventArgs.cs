using System.ComponentModel;

namespace Tabalonia.Events;

public class TabClosingEventArgs(object? item) : CancelEventArgs
{
    public object? Item { get; } = item;
}
