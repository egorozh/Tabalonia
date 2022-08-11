namespace Tabalonia;

public interface IDraggedAndRestoredWindow
{
    void DoubleTapped();
    
    void Dragged(double x, double y);
}