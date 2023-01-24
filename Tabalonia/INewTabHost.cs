namespace Tabalonia;

public interface INewTabHost<out TElement> where TElement : IAvaloniaObject
{
    TElement Container { get; }
    TabsControl TabablzControl { get; }
}