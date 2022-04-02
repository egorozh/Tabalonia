using Avalonia;
using Tabalonia.Controls;

namespace Tabalonia.Organisers;

public interface IItemsOrganiser
{
    void OrganiseOnDragStarted(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem);

    void OrganiseOnDrag(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem);

    void OrganiseOnDragCompleted( IEnumerable<DragTabItem> siblingsItems, DragTabItem dragItem);

    IEnumerable<DragTabItem> Sort(IEnumerable<DragTabItem> items);

    Point ConstrainLocation(TabsItemsPresenter requestor, Rect measureBounds, Point itemDesiredLocation);

    void Organise(Size maxConstraint, IEnumerable<DragTabItem> dragablzItems);

    void Organise(Size maxConstraint, IOrderedEnumerable<DragTabItem> dragablzItems);

    Size Measure(TabsItemsPresenter requestor, Rect bounds, IEnumerable<DragTabItem> dragablzItems);
}