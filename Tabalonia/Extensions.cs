using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
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
}