using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Threading;
using Tabalonia.Events;
using Tabalonia.Organisers;

namespace Tabalonia.Controls;

public class TabsItemsPresenter : ItemsPresenter
{
    #region Avalonia Properties

    public static readonly StyledProperty<double> AdjacentHeaderItemOffsetProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(AdjacentHeaderItemOffset), -8.0);

    public static readonly StyledProperty<IItemsOrganiser> ItemsOrganiserProperty =
        AvaloniaProperty.Register<TabsControl, IItemsOrganiser>(nameof(ItemsOrganiser), new HorizontalOrganiser());

    public static readonly StyledProperty<int> FixedItemCountProperty =
        AvaloniaProperty.Register<TabsControl, int>(nameof(FixedItemCount));

    #endregion

    #region Public Properties

    public double AdjacentHeaderItemOffset
    {
        get => GetValue(AdjacentHeaderItemOffsetProperty);
        set => SetValue(AdjacentHeaderItemOffsetProperty, value);
    }

    public IItemsOrganiser ItemsOrganiser
    {
        get => GetValue(ItemsOrganiserProperty);
        set => SetValue(ItemsOrganiserProperty, value);
    }

    public int FixedItemCount
    {
        get => GetValue(FixedItemCountProperty);
        set => SetValue(FixedItemCountProperty, value);
    }

    #endregion

    #region Constructor

    public TabsItemsPresenter()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        
        AddHandler(DragTabItem.DragStarted, ItemDragStarted);
        AddHandler(DragTabItem.DragDelta, ItemDragDelta);
        AddHandler(DragTabItem.DragCompleted, ItemDragCompleted);
        
        SetValue(ItemsOrganiserProperty, new HorizontalOrganiser(GetValue(AdjacentHeaderItemOffsetProperty)));
    }

    static TabsItemsPresenter()
    {
        AdjacentHeaderItemOffsetProperty.Changed.Subscribe(AdjacentHeaderItemOffsetPropertyChangedCallback);
    }

    #endregion

    #region Static Methods

    private static void AdjacentHeaderItemOffsetPropertyChangedCallback(AvaloniaPropertyChangedEventArgs args)
    {
        var instance = (TabsItemsPresenter)args.Sender;

        if (args.NewValue != null)
            instance.SetValue(ItemsOrganiserProperty, new HorizontalOrganiser((double)args.NewValue));
    }

    #endregion

    #region Public Methods

    public void MoveItem(MoveItemRequest moveItemRequest)
    {
        var dragablzItem = ItemContainerGenerator.FindContainer<DragTabItem>(moveItemRequest.Item); 
        var contextDragablzItem = ItemContainerGenerator.FindContainer<DragTabItem>(moveItemRequest.Context);
       
        if (dragablzItem == null) return;

        var sortedItems = DragablzItems().OrderBy(di => di.LogicalIndex).ToList();
        sortedItems.Remove(dragablzItem);

        switch (moveItemRequest.AddLocationHint)
        {
            case AddLocationHint.First:
                sortedItems.Insert(0, dragablzItem);
                break;
            case AddLocationHint.Last:
                sortedItems.Add(dragablzItem);
                break;
            case AddLocationHint.Prior:
            case AddLocationHint.After:
                if (contextDragablzItem == null)
                    return;

                var contextIndex = sortedItems.IndexOf(contextDragablzItem);
                sortedItems.Insert(moveItemRequest.AddLocationHint == AddLocationHint.Prior ? contextIndex : contextIndex + 1, dragablzItem);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //TODO might not be too great for perf on larger lists
        var orderedEnumerable = sortedItems.OrderBy(di => sortedItems.IndexOf(di));

        var maxConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
        ItemsOrganiser.Organise(maxConstraint, orderedEnumerable);
    }

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        //if (LockedMeasure.HasValue)
        //{
        //    ItemsPresenterWidth = LockedMeasure.Value.Width;
        //    ItemsPresenterHeight = LockedMeasure.Value.Height;
        //    return LockedMeasure.Value;
        //}

        var dragablzItems = DragablzItems();
        var maxConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);

        ItemsOrganiser.Organise(maxConstraint, dragablzItems);
        var measure = ItemsOrganiser.Measure(this, this.Bounds, dragablzItems);
        
        var width = !double.IsInfinity(measure.Width) ? measure.Width : availableSize.Width;
        var height = !double.IsInfinity(measure.Height) ? measure.Height : availableSize.Height;

        return new Size(width, height);
    }

    #region Internal Methods

    internal IReadOnlyList<DragTabItem> DragablzItems()
    {
        return this.ItemContainerGenerator.Containers<DragTabItem>().ToList();  
    }

    #endregion

    #region Private Methods

    private void ItemDragStarted(object? sender, DragablzDragStartedEventArgs eventArgs)
    {
        DragTabItem currentItem = eventArgs.DragablzItem;
        
        var siblingItems = DragablzItems().Except(new[] { currentItem }).ToList();
        ItemsOrganiser.OrganiseOnDragStarted(siblingItems, currentItem);

        eventArgs.Handled = true;

        Dispatcher.UIThread.Post(InvalidateMeasure, DispatcherPriority.Loaded);
    }

    private void ItemDragDelta(object? sender, DragablzDragDeltaEventArgs eventArgs)
    {
        DragTabItem currentItem = eventArgs.DragablzItem;
        
        var desiredLocation = new Point(
            currentItem.X + eventArgs.DragDeltaEventArgs.Vector.X,
            currentItem.Y + eventArgs.DragDeltaEventArgs.Vector.Y);

        if (FixedItemCount > 0 &&
            ItemsOrganiser.Sort(DragablzItems()).Take(FixedItemCount).Contains(currentItem))
        {
            eventArgs.Handled = true;
            return;
        }

        desiredLocation = ItemsOrganiser.ConstrainLocation(this, this.Bounds, desiredLocation);

        var siblingsItems = DragablzItems().Except(new[] { currentItem }).ToList();

        foreach (var dragableItem in siblingsItems)
            dragableItem.IsSiblingDragging = true;

        currentItem.IsDragging = true;

        currentItem.X = desiredLocation.X;
        currentItem.Y = desiredLocation.Y;
        
        ItemsOrganiser.OrganiseOnDrag(siblingsItems, currentItem);

        currentItem.BringIntoView();
        
        eventArgs.Handled = true;
    }

    private void ItemDragCompleted(object? sender, DragablzDragCompletedEventArgs eventArgs)
    {
        DragTabItem draggedItem = eventArgs.DragablzItem;

        var dragablzItems = DragablzItems()
            .Select(i =>
            {
                i.IsDragging = false;
                i.IsSiblingDragging = false;
                return i;
            })
            .ToList();
        
        var siblingsItems = dragablzItems.Except(new[] { draggedItem });

        ItemsOrganiser.OrganiseOnDragCompleted(siblingsItems, draggedItem);
        
        //wowsers
        Dispatcher.UIThread.Post(InvalidateMeasure);
        //Dispatcher.UIThread.Post(InvalidateMeasure, DispatcherPriority.Loaded);
        
        eventArgs.Handled = true;
    }

    #endregion

   
}