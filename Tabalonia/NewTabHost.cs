using Avalonia;
using Tabalonia.Controls;

namespace Tabalonia;

public class NewTabHost<TElement> : INewTabHost<TElement> where TElement : IAvaloniaObject
{
    public NewTabHost(TElement container, TabsControl tabablzControl)
    {
        Container = container ?? throw new ArgumentNullException(nameof(container));
        TabablzControl = tabablzControl ?? throw new ArgumentNullException(nameof(tabablzControl));
    }

    public TElement Container { get; }

    public TabsControl TabablzControl { get; }
}