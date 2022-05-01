namespace Tabalonia.Organisers;

public interface IItemsOrganiser
{
    void OrganiseOnDragStarted(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem);

    void OrganiseOnDrag(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem, IControl? addButton);

    void OrganiseOnDragCompleted( IEnumerable<DragTabItem> siblingsItems, DragTabItem dragItem);

    IEnumerable<DragTabItem> Sort(IEnumerable<DragTabItem> items);

    Point ConstrainLocation(TabsItemsPresenter requestor, Rect measureBounds, Point itemDesiredLocation, DragTabItem dragTabItem, IControl? addButton);

    void Organise(Size maxConstraint, IEnumerable<DragTabItem> dragablzItems, IControl? addButton);

    void Organise(Size maxConstraint, IOrderedEnumerable<DragTabItem> dragablzItems, IControl? addButton);

    Size Measure(TabsItemsPresenter requestor, Rect bounds, IEnumerable<DragTabItem> dragablzItems, IControl? addButton);
}