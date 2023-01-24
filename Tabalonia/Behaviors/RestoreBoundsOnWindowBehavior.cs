using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Tabalonia.Behaviors
{
    internal class RestoreBoundsOnWindowBehavior : Behavior<Window>
    {
        public Rect RestoreSize { get; private set; }

        static RestoreBoundsOnWindowBehavior()
        {
            Window.WindowStateProperty.Changed.AddClassHandler<Window>(WindowStateChanged);
            Visual.BoundsProperty.Changed.AddClassHandler<Window>(WindowBoundsChanged);
        }

        private static void WindowBoundsChanged(Window window, AvaloniaPropertyChangedEventArgs arg2)
        {
            if (window.WindowState == WindowState.Maximized)
                return;

            Interaction.GetBehaviors(window).OfType<RestoreBoundsOnWindowBehavior>()
                .First().RestoreSize = window.Bounds;
        }
        
        private static void WindowStateChanged(Window window, AvaloniaPropertyChangedEventArgs arg2)
        {
            if (arg2.NewValue is WindowState newState)
            {
                if (newState == WindowState.Maximized)
                {
                    var transBounds = window.TransformedBounds ?? new TransformedBounds(window.Bounds, new Rect(), Matrix.Identity);
                   
                    Interaction.GetBehaviors(window).OfType<RestoreBoundsOnWindowBehavior>()
                        .First().RestoreSize = transBounds.Bounds;
                }
                else
                {
                    Interaction.GetBehaviors(window).OfType<RestoreBoundsOnWindowBehavior>()
                        .First().RestoreSize = window.Bounds;
                }
            }
        }
    }
}
