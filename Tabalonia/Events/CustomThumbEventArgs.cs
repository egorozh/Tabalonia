using Avalonia.Input;
using Avalonia.Interactivity;

namespace Tabalonia.Events;

public class CustomThumbEventArgs : RoutedEventArgs
{
    public Vector Vector { get; set; }

    public PointerEventArgs PointerEventArgs { get; }


    public CustomThumbEventArgs(PointerEventArgs pointerEventArgs)
    {
        PointerEventArgs = pointerEventArgs;
    }

    public CustomThumbEventArgs(PointerCaptureLostEventArgs pointerCaptureLostEventArgs)
    {
        PointerEventArgs = new PointerEventArgs(
            pointerCaptureLostEventArgs.RoutedEvent, 
            pointerCaptureLostEventArgs.Source,
            pointerCaptureLostEventArgs.Pointer, 
            null, 
            new Point(),
            0, 
            PointerPointProperties.None, 
            KeyModifiers.None);
    }
}