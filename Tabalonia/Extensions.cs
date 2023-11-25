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


    public static void RestoreWindow(this Window? window)
    {
        if (window is null)
            return;

        window.WindowState = WindowState.Maximized;
    }


    public static void DragWindow(this Window? window, double vectorX, double vectorY)
    {
        if (window is null)
            return;

        var pos = window.Position;
        
        window.Position = new PixelPoint(
            x: (int) (pos.X + vectorX),
            y: (int) (pos.Y + vectorY));
    }
}