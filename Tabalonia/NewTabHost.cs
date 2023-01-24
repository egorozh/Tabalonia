namespace Tabalonia;

public class NewTabHost<TElement> : INewTabHost<TElement> where TElement : IAvaloniaObject
{
    public TElement Container { get; }

    public TabsControl TabablzControl { get; }


    public NewTabHost(TElement container, TabsControl tabablzControl)
    {
        ArgumentNullException.ThrowIfNull(container, nameof(container));
        ArgumentNullException.ThrowIfNull(tabablzControl, nameof(tabablzControl));

        Container = container;
        TabablzControl = tabablzControl;
    }
}