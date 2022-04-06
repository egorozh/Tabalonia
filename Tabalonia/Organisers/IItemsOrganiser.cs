using Avalonia;
using Avalonia.Controls;
using Tabalonia.Controls;

namespace Tabalonia.Organisers;

public interface IItemsOrganiser
{
    void OrganiseOnDragStarted(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem);

    void OrganiseOnDrag(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem, Button? addButton);

    void OrganiseOnDragCompleted( IEnumerable<DragTabItem> siblingsItems, DragTabItem dragItem);

    IEnumerable<DragTabItem> Sort(IEnumerable<DragTabItem> items);

    Point ConstrainLocation(TabsItemsPresenter requestor, Rect measureBounds, Point itemDesiredLocation,
        DragTabItem dragTabItem, Button? addButton);

    void Organise(Size maxConstraint, IEnumerable<DragTabItem> dragablzItems, Button? addButton);

    void Organise(Size maxConstraint, IOrderedEnumerable<DragTabItem> dragablzItems, Button? addButton);

    Size Measure(TabsItemsPresenter requestor, Rect bounds, IEnumerable<DragTabItem> dragablzItems, Button? addButton);
}