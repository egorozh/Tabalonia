using Avalonia;
using Avalonia.Controls;
using Tabalonia.Controls;

namespace Tabalonia.Organisers;

public interface IItemsOrganiser
{
    void OrganiseOnDragStarted(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem);

    void OrganiseOnDrag(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem, IControl? addButton, IControl? dragThumb);

    void OrganiseOnDragCompleted( IEnumerable<DragTabItem> siblingsItems, DragTabItem dragItem);

    IEnumerable<DragTabItem> Sort(IEnumerable<DragTabItem> items);

    Point ConstrainLocation(TabsItemsPresenter requestor, Rect measureBounds, Point itemDesiredLocation,
        DragTabItem dragTabItem, IControl? addButton, IControl? dragThumb);

    void Organise(Size maxConstraint, IEnumerable<DragTabItem> dragablzItems, IControl? addButton, IControl? dragThumb);

    void Organise(Size maxConstraint, IOrderedEnumerable<DragTabItem> dragablzItems, IControl? addButton, IControl? dragThumb);

    Size Measure(TabsItemsPresenter requestor, Rect bounds, IEnumerable<DragTabItem> dragablzItems, IControl? addButton, IControl dragThumb);
}