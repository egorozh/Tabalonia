using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Tabalonia.Exceptions;

namespace Tabalonia;

internal static class Extensions
{
    public static T Find<T>(this TemplateAppliedEventArgs e, string elementName) where T : class
    {
        var element = e.NameScope.Find<T>(elementName);

        if (element == null)
            throw new ElementNotFoundOnStyleException(elementName);

        return element;
    }

    public static IEnumerable<TContainer> Containers<TContainer>(this IItemContainerGenerator itemGen)
        where TContainer : class
    {
        foreach (ItemContainerInfo? info in itemGen.Containers)
        {
            if (info.ContainerControl is TContainer c)
                yield return c;
        }
    }

    public static TContainer? FindContainer<TContainer>(this IItemContainerGenerator itemGen, object? item)
        where TContainer : class
    {
        if (item == null)
            return null;

        var info = itemGen.Containers.FirstOrDefault(i => i.Item == item);

        if (info is {ContainerControl: TContainer c})
            return c;

        return null;
    }

    public static object? FindItem<TContainer>(this IItemContainerGenerator itemGen, TContainer? container)
        where TContainer : class
    {
        if (container == null)
            return null;

        var info = itemGen.Containers.FirstOrDefault(i => i.ContainerControl == container);

        if (info is {Item: { } item})
            return item;

        return null;
    }

    /// <summary>
    /// Yields the visual ancestory (including the starting point).
    /// </summary>
    /// <param name="dependencyObject"></param>
    /// <returns></returns>
    public static IEnumerable<IVisual> VisualTreeAncestory(this IVisual dependencyObject)
    {
        if (dependencyObject == null) throw new ArgumentNullException(nameof(dependencyObject));

        while (dependencyObject != null)
        {
            yield return dependencyObject;
            dependencyObject = dependencyObject.GetVisualParent();
        }
    }

    /// <summary>
    /// Yields the logical ancestory (including the starting point).
    /// </summary>
    /// <param name="dependencyObject"></param>
    /// <returns></returns>
    public static IEnumerable<ILogical> LogicalTreeAncestory(this ILogical dependencyObject)
    {
        if (dependencyObject == null) throw new ArgumentNullException(nameof(dependencyObject));

        while (dependencyObject != null)
        {
            yield return dependencyObject;
            dependencyObject = dependencyObject.GetLogicalParent();
        }
    }

    public static Window? GetWindow(this ILogical dependencyObject)
        => dependencyObject.LogicalTreeAncestory().OfType<Window>().FirstOrDefault();

    public static IEnumerable<object> LogicalTreeDepthFirstTraversal(this ILogical node)
    {
        if (node == null)
            yield break;

        yield return node;

        foreach (var child in node.LogicalChildren.OfType<ILogical>()
                     .SelectMany(depObj => depObj.LogicalTreeDepthFirstTraversal()))
            yield return child;
    }

    public static IEnumerable<object> VisualTreeDepthFirstTraversal(this IVisual node)
    {
        if (node == null) yield break;
        yield return node;

        foreach (var child in node.VisualChildren)
        {
            foreach (var d in child.VisualTreeDepthFirstTraversal())
            {
                yield return d;
            }
        }
    }

    public static IEnumerable<TObject> Except<TObject>(this IEnumerable<TObject> first, params TObject[] second)
    {
        return first.Except((IEnumerable<TObject>)second);
    }
}