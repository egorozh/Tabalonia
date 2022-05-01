using Avalonia.Layout;

namespace Tabalonia;

internal class InterTabTransfer
{
    public InterTabTransfer(object item, DragTabItem originatorContainer, Orientation breachOrientation, Point dragStartWindowOffset, Point dragStartItemOffset, Point? itemPositionWithinHeader, Rect itemSize, 
         bool isTransposing)
    {
        TransferReason = InterTabTransferReason.Breach;

        Item = item ?? throw new ArgumentNullException(nameof(item));
        OriginatorContainer = originatorContainer ?? throw new ArgumentNullException(nameof(originatorContainer));
        BreachOrientation = breachOrientation;
        DragStartWindowOffset = dragStartWindowOffset;
        DragStartItemOffset = dragStartItemOffset;
        ItemPositionWithinHeader = itemPositionWithinHeader;
        ItemSize = itemSize;
        IsTransposing = isTransposing;
    }

    public InterTabTransfer(object item, DragTabItem originatorContainer, Point dragStartItemOffset)
    {
        TransferReason = InterTabTransferReason.Reentry;

        Item = item ?? throw new ArgumentNullException(nameof(item));
        OriginatorContainer = originatorContainer ?? throw new ArgumentNullException(nameof(originatorContainer));
        DragStartItemOffset = dragStartItemOffset;
    }

    public Orientation BreachOrientation { get; }

    public Point DragStartWindowOffset { get; }

    public object Item { get; }

    public DragTabItem OriginatorContainer { get; }

    public InterTabTransferReason TransferReason { get; }

    public Point DragStartItemOffset { get; }

    public Point? ItemPositionWithinHeader { get; }

    public Rect ItemSize { get; }
    
    public bool IsTransposing { get; }
}