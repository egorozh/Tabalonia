using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Tabalonia.Events;

namespace Tabalonia.Controls;

public class TabsControl : TabControl
{
    #region Internal Fields

    internal TabsItemsPresenter ItemsPresenter = null!;

    #endregion

    #region Constructor

    public TabsControl()
    {
        ItemsPanel = new FuncTemplate<IPanel>(() => new Canvas());
        AddHandler(DragTabItem.DragStarted, ItemDragStarted, handledEventsToo: true);
        AddHandler(DragTabItem.DragCompleted, ItemDragCompleted, handledEventsToo: true);
    }
    
    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ItemsPresenter = e.NameScope.Get<TabsItemsPresenter>("PART_ItemsPresenter");
    }

    protected override IItemContainerGenerator CreateItemContainerGenerator()
        => new DragTabItemContainerGenerator(this);

    protected override void OnContainersMaterialized(ItemContainerEventArgs e)
    {
        base.OnContainersMaterialized(e);

        if (e.Containers.FirstOrDefault() is {ContainerControl: DragTabItem exTabItem} info)
        {
            exTabItem.TabIndex = info.Index;
            exTabItem.LogicalIndex = info.Index;
        }
    }

    #endregion

    private void ItemDragStarted(object? sender, DragablzDragStartedEventArgs e)
    {
        var draggedItem = e.DragablzItem;

        //e.DragablzItem.IsDropTargetFound = false;

        //var sourceOfDragItemsControl = ItemsControlFromItemContainer(e.DragablzItem) as DragablzItemsControl;
        //if (sourceOfDragItemsControl == null || !Equals(sourceOfDragItemsControl, _dragablzItemsControl)) return;

        //var itemsControlOffset = Mouse.GetPosition(_dragablzItemsControl);
        //_tabHeaderDragStartInformation = new TabHeaderDragStartInformation(e.DragablzItem, itemsControlOffset.X,
        //    itemsControlOffset.Y, e.DragStartedEventArgs.HorizontalOffset, e.DragStartedEventArgs.VerticalOffset);

        var siblingsItems = ItemsPresenter.DragablzItems().Except(new[] {draggedItem});

        foreach (var otherItem in siblingsItems)
            otherItem.IsSelected = false;

        draggedItem.IsSelected = true;

        var itemInfo = ItemContainerGenerator.Containers.FirstOrDefault(c => Equals(c.ContainerControl, draggedItem));

        if (itemInfo != null)
        {
            var item = itemInfo.Item;

            if (itemInfo.Item is TabItem tabItem)
                tabItem.IsSelected = true;

            SelectedItem = item;
        }

        //e.DragablzItem.PartitionAtDragStart = InterTabController?.Partition;

        //if (ShouldDragWindow(sourceOfDragItemsControl))
        //    IsDraggingWindow = true;
    }

    private void ItemDragCompleted(object? sender, DragablzDragCompletedEventArgs e)
    {

    }
}