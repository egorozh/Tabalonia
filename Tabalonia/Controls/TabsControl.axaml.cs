using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Collections;
using System.Collections.Specialized;
using Avalonia.Layout;
using Tabalonia.Events;

namespace Tabalonia.Controls;

public class TabsControl : TabControl
{
    private Point _pointerPositionOnHeaderItemsControl;

    #region Internal Fields

    internal TabsItemsPresenter? ItemsPresenter;

    #endregion

    #region Avalonia Properties

    public static readonly StyledProperty<bool> ShowDefaultAddButtonProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultAddButton));

    public static readonly StyledProperty<bool> ShowDefaultCloseButtonProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultCloseButton));

    /// <summary>
    /// Allows a factory to be provided for generating new items. Typically used in conjunction with <see cref="AddItem"/>.
    /// </summary>
    public static readonly StyledProperty<Func<object>> NewItemFactoryProperty =
        AvaloniaProperty.Register<TabsControl, Func<object>>(nameof(NewItemFactory));

    public static readonly StyledProperty<int> FixedHeaderCountProperty =
        AvaloniaProperty.Register<TabsControl, int>(nameof(FixedHeaderCount));

    public static readonly StyledProperty<Template?> AddButtonTemplateProperty =
        AvaloniaProperty.Register<TabsControl, Template?>(nameof(AddButtonTemplate));

    public static readonly StyledProperty<InterTabController?> InterTabControllerProperty =
        AvaloniaProperty.Register<TabsControl, InterTabController?>(nameof(InterTabController));

    #endregion

    #region Public Properties

    /// <summary>
    /// An <see cref="InterTabController"/> must be provided to enable tab tearing. Behaviour customisations can be applied
    /// vie the controller.
    /// </summary>
    public InterTabController? InterTabController
    {
        get => GetValue(InterTabControllerProperty);
        set => SetValue(InterTabControllerProperty, value);
    }

    public bool ShowDefaultAddButton
    {
        get => GetValue(ShowDefaultAddButtonProperty);
        set => SetValue(ShowDefaultAddButtonProperty, value);
    }

    /// <summary>
    /// Indicates whether a default close button should be displayed.  If manually templating the tab header content the close command 
    /// can be called by executing the <see cref="CloseItem"/> command (typically via a <see cref="Button"/>).
    /// </summary>
    public bool ShowDefaultCloseButton
    {
        get => GetValue(ShowDefaultCloseButtonProperty);
        set => SetValue(ShowDefaultCloseButtonProperty, value);
    }

    /// <summary>
    /// Allows a factory to be provided for generating new items. Typically used in conjunction with <see cref="AddItem"/>.
    /// </summary>
    public Func<object> NewItemFactory
    {
        get => GetValue(NewItemFactoryProperty);
        set => SetValue(NewItemFactoryProperty, value);
    }

    /// <summary>
    /// Allows a the first adjacent tabs to be fixed (no dragging, and default close button will not show).
    /// </summary>
    public int FixedHeaderCount
    {
        get => GetValue(FixedHeaderCountProperty);
        set => SetValue(FixedHeaderCountProperty, value);
    }

    public Template? AddButtonTemplate
    {
        get => GetValue(AddButtonTemplateProperty);
        set => SetValue(AddButtonTemplateProperty, value);
    }

    #endregion

    #region Constructor

    static TabsControl()
    {
        ShowDefaultAddButtonProperty.Changed.Subscribe(OnShowDefaultAddButtonChanged);
        AddButtonTemplateProperty.Changed.Subscribe(OnAddButtonTemplateChanged);
    }

    public TabsControl()
    {
        ItemsPanel = new FuncTemplate<IPanel>(() => new Canvas());
        AddHandler(DragTabItem.DragStarted, ItemDragStarted, handledEventsToo: true);
        AddHandler(DragTabItem.PreviewDragDelta, PreviewItemDragDelta, handledEventsToo: true);
        AddHandler(DragTabItem.DragDelta, ItemDragDelta, handledEventsToo: true);
        AddHandler(DragTabItem.DragCompleted, ItemDragCompleted, handledEventsToo: true);
    }

    #endregion


    #region Public Methods

    public void AddItem()
    {
        if (NewItemFactory == null)
            throw new InvalidOperationException("NewItemFactory must be provided.");

        var newItem = NewItemFactory();
        if (newItem == null) throw new ApplicationException("NewItemFactory returned null.");

        AddToSource(newItem);
        SelectedItem = newItem;

        Dispatcher.UIThread.Post(ItemsPresenter.InvalidateMeasure, DispatcherPriority.Loaded);
    }

    public void CloseItem(object tabItemSource)
    {
        if (tabItemSource == null)
            throw new ApplicationException("Valid DragablzItem to close is required.");

        var tabItem = FindDragTabItem(tabItemSource);

        if (tabItem == null)
            return;

        var cancel = false;

        //if (ClosingItemCallback != null)
        //{
        //    var callbackArgs = new ItemActionCallbackArgs<TabablzControl>(Window.GetWindow(owner), owner, item);
        //    ClosingItemCallback(callbackArgs);
        //    cancel = callbackArgs.IsCancelled;
        //}

        if (!cancel)
            RemoveItem(tabItem);
    }

    /// <summary>
    /// Adds an item to the source collection.  If the InterTabController.InterTabClient is set that instance will be deferred to.
    /// Otherwise an attempt will be made to add to the <see cref="ItemsSource" /> property, and lastly <see cref="Items"/>.
    /// </summary>
    /// <param name="item"></param>
    public void AddToSource(object item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        if (Items is IList itemsList)
            itemsList.Add(item);

        //var manualInterTabClient = InterTabController == null ? null : InterTabController.InterTabClient as IManualInterTabClient;
        //if (manualInterTabClient != null)
        //{
        //    manualInterTabClient.Add(item);
        //}
        //else
        //{
        //    CollectionTeaser collectionTeaser;
        //    if (CollectionTeaser.TryCreate(ItemsSource, out collectionTeaser))
        //        collectionTeaser.Add(item);
        //    else
        //        Items.Add(item);
        //}
    }

    public void RemoveFromSource(object item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        if (Items is IList itemsList)
            itemsList.Remove(item);

        //var manualInterTabClient = InterTabController == null ? null : InterTabController.InterTabClient as IManualInterTabClient;
        //if (manualInterTabClient != null)
        //{
        //    manualInterTabClient.Remove(item);
        //}
        //else
        //{
        //    CollectionTeaser collectionTeaser;
        //    if (CollectionTeaser.TryCreate(ItemsSource, out collectionTeaser))
        //        collectionTeaser.Remove(item);
        //    else
        //        Items.Remove(item);
        //}
    }

    #endregion

    protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        base.ItemsCollectionChanged(sender, e);

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems[0] != null)
                if (ItemsPresenter != null)
                    ItemsPresenter.MoveItem(new MoveItemRequest(e.NewItems[0], SelectedItem, AddLocationHint.Last));
        }
    }

    #region Internal Methods

    internal object RemoveItem(DragTabItem container)
    {
        var containterInfo =
            ItemContainerGenerator.Containers.FirstOrDefault(c => Equals(c.ContainerControl, container));

        if (containterInfo == null)
            return container;

        var item = containterInfo.Item;

        //stop the header shrinking if the tab stays open when empty
        //var minSize = EmptyHeaderSizingHint == EmptyHeaderSizingHint.PreviousTab
        //    ? new Size(_dragablzItemsControl.ActualWidth, _dragablzItemsControl.ActualHeight)
        //    : new Size();

        //_dragablzItemsControl.MinHeight = 0;
        //_dragablzItemsControl.MinWidth = 0;

        //var contentPresenter = FindChildContentPresenter(item);
        RemoveFromSource(item);
        //_itemsHolder.Children.Remove(contentPresenter);

        //if (Items.Count != 0) return item;

        //var window = Window.GetWindow(this);

        //if (window != null
        //    && InterTabController != null
        //    && InterTabController.InterTabClient.TabEmptiedHandler(this, window) == TabEmptiedResponse.CloseWindowOrLayoutBranch)
        //{
        //    if (Layout.ConsolidateBranch(this)) return item;

        //    try
        //    {
        //        SetIsClosingAsPartOfDragOperation(window, true);
        //        window.Close();
        //    }
        //    finally
        //    {
        //        SetIsClosingAsPartOfDragOperation(window, false);
        //    }
        //}
        //else
        //{
        //    _dragablzItemsControl.MinHeight = minSize.Height;
        //    _dragablzItemsControl.MinWidth = minSize.Width;
        //}

        return item;
    }

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ItemsPresenter = e.NameScope.Get<TabsItemsPresenter>("PART_ItemsPresenter");

        CreateAddButton();
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

    #region Private Static Methods

    private static void OnShowDefaultAddButtonChanged(AvaloniaPropertyChangedEventArgs args)
    {
        ((TabsControl) args.Sender).CreateAddButton();
    }

    private static void OnAddButtonTemplateChanged(AvaloniaPropertyChangedEventArgs args)
    {
        ((TabsControl) args.Sender).CreateAddButton();
    }

    private static DragTabItem? FindDragTabItem(object originalSource)
    {
        if (originalSource is DragTabItem dragTabItem)
            return dragTabItem;

        if (originalSource is IVisual visual &&
            visual.VisualTreeAncestory().OfType<DragTabItem>().FirstOrDefault() is { } item)
        {
            return item;
        }

        if (originalSource is ILogical logical &&
            logical.LogicalTreeAncestory().OfType<Popup>().LastOrDefault() is {PlacementTarget: { } placementTarget})
        {
            return placementTarget.VisualTreeAncestory().OfType<DragTabItem>().FirstOrDefault();
        }

        return null;
    }

    private static TabsItemsPresenter? ItemsPresenterFromItemContainer(DragTabItem tabItem)
        => tabItem.VisualTreeAncestory().OfType<TabsItemsPresenter>().LastOrDefault();

    private static TabsControl? TabsControlFromItemContainer(DragTabItem tabItem)
        => tabItem.VisualTreeAncestory().OfType<TabsControl>().LastOrDefault();

    #endregion

    #region Private Methods

    private void CreateAddButton()
    {
        if (ItemsPresenter == null)
            return;

        if (ShowDefaultAddButton)
            ItemsPresenter.AddButton = AddButtonTemplate?.Build() ?? new Button {Content = "+"};
        else
            ItemsPresenter.AddButton = null;
    }


    private void ItemDragStarted(object? sender, DragablzDragStartedEventArgs e)
    {
        var draggedItem = e.DragablzItem;

        if (!IsMyItem(draggedItem)) return;

        //e.DragablzItem.IsDropTargetFound = false;

        var parentPresenter = ItemsPresenterFromItemContainer(draggedItem);

        if (parentPresenter is not { } sourceOfDragItemsControl
            || !Equals(sourceOfDragItemsControl, ItemsPresenter)) return;

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

    private void PreviewItemDragDelta(object? sender, DragablzDragDeltaEventArgs e)
    {
        var draggedItem = e.DragablzItem;

        var parentPresenter = ItemsPresenterFromItemContainer(draggedItem);

        if (parentPresenter is not { } sourceOfDragItemsControl
            || !Equals(sourceOfDragItemsControl, ItemsPresenter)) return;

        if (MonitorReentry(e))
            return;

        var myWindow = this.GetWindow();
        if (myWindow == null) return;
    }

    private void ItemDragDelta(object? sender, DragablzDragDeltaEventArgs e)
    {
        var draggedItem = e.DragablzItem;

        if (!IsMyItem(draggedItem)) return;

        if (FixedHeaderCount > 0 &&
            ItemsPresenter.ItemsOrganiser.Sort(ItemsPresenter.DragablzItems())
                .Take(FixedHeaderCount)
                .Contains(draggedItem))
            return;

        //if (_tabHeaderDragStartInformation == null ||
        //    !Equals(_tabHeaderDragStartInformation.DragItem, e.DragablzItem) || InterTabController == null) return;

        //if (InterTabController.InterTabClient == null)
        //    throw new InvalidOperationException("An InterTabClient must be provided on an InterTabController.");

        MonitorBreach(e);
    }

    private void ItemDragCompleted(object? sender, DragablzDragCompletedEventArgs e)
    {
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        _pointerPositionOnHeaderItemsControl = e.GetPosition(ItemsPresenter);
    }

    private void MonitorBreach(DragablzDragDeltaEventArgs e)
    {
        //var horizontalPopoutGrace = InterTabController.HorizontalPopoutGrace;
        //var verticalPopoutGrace = InterTabController.VerticalPopoutGrace;
        var horizontalPopoutGrace = 8;
        var verticalPopoutGrace = 8;

        Orientation? breachOrientation = null;
        if (_pointerPositionOnHeaderItemsControl.X < -horizontalPopoutGrace
            || (_pointerPositionOnHeaderItemsControl.X - ItemsPresenter.Bounds.Width) > horizontalPopoutGrace)
            breachOrientation = Orientation.Horizontal;
        else if (_pointerPositionOnHeaderItemsControl.Y < -verticalPopoutGrace
                 || (_pointerPositionOnHeaderItemsControl.Y - ItemsPresenter.Bounds.Height) > verticalPopoutGrace)
            breachOrientation = Orientation.Vertical;

        if (!breachOrientation.HasValue)
            return;

        var newTabHost = InterTabController.InterTabClient.GetNewHost(InterTabController.InterTabClient,
            InterTabController.Partition, this);

        if (newTabHost?.TabablzControl == null || newTabHost.Container == null)
            throw new ApplicationException("New tab host was not correctly provided");

        var item = ItemsPresenter.ItemContainerGenerator.FindItem(e.DragablzItem);
        var isTransposing = IsTransposing(newTabHost.TabablzControl);

        var myWindow = this.GetWindow();
        if (myWindow == null) throw new ApplicationException("Unable to find owning window.");

        /*
                           
         var dragStartWindowOffset = ConfigureNewHostSizeAndGetDragStartWindowOffset(myWindow, newTabHost, e.DragablzItem, isTransposing);

         var dragableItemHeaderPoint = e.DragablzItem.TranslatePoint(new Point(), _dragablzItemsControl);
         var dragableItemSize = new Size(e.DragablzItem.ActualWidth, e.DragablzItem.ActualHeight);
         var floatingItemSnapShots = this.VisualTreeDepthFirstTraversal()
             .OfType<Layout>()
             .SelectMany(l => l.FloatingDragablzItems().Select(FloatingItemSnapShot.Take))
             .ToList();            

         var interTabTransfer = new InterTabTransfer(item, e.DragablzItem, breachOrientation.Value, dragStartWindowOffset, e.DragablzItem.MouseAtDragStart, dragableItemHeaderPoint, dragableItemSize, floatingItemSnapShots, isTransposing);

         if (myWindow.WindowState == WindowState.Maximized)
         {
             var desktopMousePosition = Native.GetCursorPos().ToWpf();
             newTabHost.Container.Left = desktopMousePosition.X - dragStartWindowOffset.X;
             newTabHost.Container.Top = desktopMousePosition.Y - dragStartWindowOffset.Y;
         }
         else
         {
             newTabHost.Container.Left = myWindow.Left;
             newTabHost.Container.Top = myWindow.Top;
         }
         newTabHost.Container.Show();
         var contentPresenter = FindChildContentPresenter(item);

         //stop the header shrinking if the tab stays open when empty
         var minSize = EmptyHeaderSizingHint == EmptyHeaderSizingHint.PreviousTab
             ? new Size(_dragablzItemsControl.ActualWidth, _dragablzItemsControl.ActualHeight)
             : new Size();
         System.Diagnostics.Debug.WriteLine("B " + minSize);

         RemoveFromSource(item);
         _itemsHolder.Children.Remove(contentPresenter);
         if (Items.Count == 0)
         {
             _dragablzItemsControl.MinHeight = minSize.Height;
             _dragablzItemsControl.MinWidth = minSize.Width;
             Layout.ConsolidateBranch(this);
         }

         RestorePreviousSelection();

         foreach (var dragablzItem in _dragablzItemsControl.DragablzItems())
         {
             dragablzItem.IsDragging = false;
             dragablzItem.IsSiblingDragging = false;
         }

         newTabHost.TabablzControl.ReceiveDrag(interTabTransfer);
         interTabTransfer.OriginatorContainer.IsDropTargetFound = true;
         
         */

        e.Cancel = true;
    }

    private bool MonitorReentry(DragablzDragDeltaEventArgs e)
    {
        var draggedItem = e.DragablzItem;

        var sourceTabablzControl = TabsControlFromItemContainer(draggedItem);

        if (sourceTabablzControl.ItemsPresenter.DragablzItems().Count > 1 &&
            draggedItem.LogicalIndex < sourceTabablzControl.FixedHeaderCount)
            return false;

        /*
       

        var otherTabablzControls = LoadedInstances
            .Where(
                tc =>
                    tc != this && tc.InterTabController != null && InterTabController != null
                    && Equals(tc.InterTabController.Partition, InterTabController.Partition)
                    && tc._dragablzItemsControl != null)
            .Select(tc =>
            {
                var topLeft = tc._dragablzItemsControl.PointToScreen(new Point());
                var lastFixedItem = tc._dragablzItemsControl.DragablzItems()
                    .OrderBy(di => di.LogicalIndex)
                    .Take(tc._dragablzItemsControl.FixedItemCount)
                    .LastOrDefault();
                //TODO work this for vert tabs
                if (lastFixedItem != null)
                    topLeft.Offset(lastFixedItem.X + lastFixedItem.ActualWidth, 0);
                var bottomRight =
                    tc._dragablzItemsControl.PointToScreen(new Point(tc._dragablzItemsControl.ActualWidth,
                        tc._dragablzItemsControl.ActualHeight));

                return new { tc, topLeft, bottomRight };
            });

        var screenMousePosition = _dragablzItemsControl.PointToScreen(Mouse.GetPosition(_dragablzItemsControl));

        var target = Native.SortWindowsTopToBottom(Application.Current.Windows.OfType<Window>())
            .Join(otherTabablzControls, w => w, a => Window.GetWindow(a.tc), (w, a) => a)
            .FirstOrDefault(a => new Rect(a.topLeft, a.bottomRight).Contains(screenMousePosition));

        if (target == null) return false;

        var mousePositionOnItem = Mouse.GetPosition(e.DragablzItem);

        var floatingItemSnapShots = this.VisualTreeDepthFirstTraversal()
            .OfType<Layout>()
            .SelectMany(l => l.FloatingDragablzItems().Select(FloatingItemSnapShot.Take))
            .ToList();

        e.DragablzItem.IsDropTargetFound = true;
        var item = RemoveItem(e.DragablzItem);

        var interTabTransfer = new InterTabTransfer(item, e.DragablzItem, mousePositionOnItem, floatingItemSnapShots);
        e.DragablzItem.IsDragging = false;

        target.tc.ReceiveDrag(interTabTransfer);
       
        */

        e.Cancel = true;
        return true;
    }


    private bool IsMyItem(DragTabItem item) => ItemsPresenter.DragablzItems().Contains(item);


    private bool IsTransposing(TabControl target) => IsVertical(this) != IsVertical(target);

    private static bool IsVertical(TabControl tabControl) => tabControl.TabStripPlacement is Dock.Left or Dock.Right;

    #endregion
}