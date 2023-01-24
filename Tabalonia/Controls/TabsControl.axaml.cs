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
using System.ComponentModel;
using System.Reactive.Disposables;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Xaml.Interactivity;
using Tabalonia.Behaviors;
using Tabalonia.Controls.Docking;
using Tabalonia.Events;

namespace Tabalonia.Controls;

public class TabsControl : TabControl
{
    #region Private Fields

    private static readonly HashSet<TabsControl> LoadedInstances = new();

    private InterTabTransfer? _interTabTransfer;
    private WeakReference _previousSelection;
    private IDisposable _windowSubscriptionDisposable;
    private DragTabItem? _dragStartItem;

    #endregion

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
        this.SelectionChanged += OnSelectionChanged;
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        LoadedInstances.Add(this);
        var window = this.GetWindow();
        if (window == null)
            return;

        window.Closing += WindowOnClosing;
        _windowSubscriptionDisposable = Disposable.Create(() => window.Closing -= WindowOnClosing);

        var behavior = new RestoreBoundsOnWindowBehavior();
        Interaction.GetBehaviors(window).Add(behavior);
    }

    private void WindowOnClosing(object? sender, CancelEventArgs e)
    {
        _windowSubscriptionDisposable.Dispose();
        _windowSubscriptionDisposable = Disposable.Empty;

        //if (!ConsolidateOrphanedItems || InterTabController == null) return;

        var window = (Window) sender;

        var orphanedItems = ItemsPresenter.DragablzItems();

        //if (ConsolidatingOrphanedItemCallback != null)
        //{
        //    orphanedItems =
        //        orphanedItems.Where(
        //            di =>
        //            {
        //                var args = new ItemActionCallbackArgs<TabablzControl>(window, this, di);
        //                ConsolidatingOrphanedItemCallback(args);
        //                return !args.IsCancelled;
        //            }).ToList();
        //}

        var target =
            LoadedInstances.Except(this)
                .FirstOrDefault(
                    other =>
                        other.InterTabController != null &&
                        other.InterTabController.Partition == InterTabController.Partition);
        if (target == null) return;

        foreach (var item in orphanedItems.Select(orphanedItem =>
                     ItemsPresenter.ItemContainerGenerator.FindItem(orphanedItem)))
        {
            RemoveFromSource(item);
            target.AddToSource(item);
        }

        var b = Interaction.GetBehaviors(window).OfType<RestoreBoundsOnWindowBehavior>().First();
        Interaction.GetBehaviors(window).Remove(b);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _windowSubscriptionDisposable.Dispose();
        _windowSubscriptionDisposable = Disposable.Empty;
        LoadedInstances.Remove(this);
    }

    protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        base.ItemsCollectionChanged(sender, e);

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems[0] != null)
            {
                ItemsPresenter?.MoveItem(new MoveItemRequest(e.NewItems[0], SelectedItem, AddLocationHint.Last));
            }
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

    internal void ReceiveDrag(InterTabTransfer interTabTransfer, PointerEventArgs pointerEventArgs)
    {
        var myWindow = this.GetWindow();

        if (myWindow == null)
            throw new ApplicationException("Unable to find owning window.");

        myWindow.Activate();

        _interTabTransfer = interTabTransfer;

        if (ItemsPresenter.DragablzItems().Count == 0)
        {
            if (interTabTransfer.IsTransposing)
                ItemsPresenter.LockedMeasure = new Size(
                    interTabTransfer.ItemSize.Width,
                    interTabTransfer.ItemSize.Height);
            else
                ItemsPresenter.LockedMeasure = new Size(
                    interTabTransfer.ItemPositionWithinHeader.X + interTabTransfer.ItemSize.Width,
                    interTabTransfer.ItemPositionWithinHeader.Y + interTabTransfer.ItemSize.Height);
        }

        //if (Items.Count == 0)
        //{
        //    if (interTabTransfer.IsTransposing)
        //        _dragablzItemsControl.LockedMeasure = new Size(
        //            interTabTransfer.ItemSize.Width,
        //            interTabTransfer.ItemSize.Height);
        //    else
        //        _dragablzItemsControl.LockedMeasure = new Size(
        //            interTabTransfer.ItemPositionWithinHeader.X + interTabTransfer.ItemSize.Width,
        //            interTabTransfer.ItemPositionWithinHeader.Y + interTabTransfer.ItemSize.Height);
        //}

        var lastFixedItem = ItemsPresenter.DragablzItems()
            .OrderBy(i => i.LogicalIndex)
            .Take(ItemsPresenter.FixedItemCount)
            .LastOrDefault();

        AddToSource(interTabTransfer.Item);

        SelectedItem = interTabTransfer.Item;

        //Dispatcher.BeginInvoke(new Action(() => Layout.RestoreFloatingItemSnapShots(this, interTabTransfer.FloatingItemSnapShots)), DispatcherPriority.Loaded);

        ItemsPresenter.InstigateDrag(interTabTransfer.Item, pointerEventArgs, newContainer =>
        {
            //newContainer.PartitionAtDragStart = interTabTransfer.OriginatorContainer.PartitionAtDragStart;
            //newContainer.IsDropTargetFound = true;

            if (interTabTransfer.TransferReason == InterTabTransferReason.Breach)
            {
                if (interTabTransfer.IsTransposing)
                {
                    newContainer.Y = 0;
                    newContainer.X = 0;
                }
                else
                {
                    newContainer.Y = interTabTransfer.OriginatorContainer.Y;
                    newContainer.X = interTabTransfer.OriginatorContainer.X;
                }
            }
            else
            {
                if (TabStripPlacement is Dock.Top or Dock.Bottom)
                {
                    var mouseXOnItemsControl = pointerEventArgs.GetPosition(null).X -
                                               ItemsPresenter.PointToScreen(new Point()).X;

                    var newX = mouseXOnItemsControl - interTabTransfer.DragStartItemOffset.X;

                    if (lastFixedItem != null)
                    {
                        newX = Math.Max(newX, lastFixedItem.X + lastFixedItem.Bounds.Width);
                    }

                    newContainer.X = newX;
                    newContainer.Y = 0;
                }
                else
                {
                    //var mouseYOnItemsControl = Native.GetCursorPos().Y -
                    //                           _dragablzItemsControl.PointToScreen(new Point()).Y;
                    //var newY = mouseYOnItemsControl - interTabTransfer.DragStartItemOffset.Y;
                    //if (lastFixedItem != null)
                    //{
                    //    newY = Math.Max(newY, lastFixedItem.Y + lastFixedItem.ActualHeight);
                    //}
                    //newContainer.X = 0;
                    //newContainer.Y = newY;
                }
            }

            newContainer.MouseAtDragStart = interTabTransfer.DragStartItemOffset;
        });
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

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count > 0 && e.AddedItems.Count > 0)
            _previousSelection = new WeakReference(e.RemovedItems[0]);
    }

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

        _dragStartItem = draggedItem;

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

        TabsItemsPresenter? parentPresenter = ItemsPresenterFromItemContainer(draggedItem);

        if (parentPresenter is null || !Equals(parentPresenter, ItemsPresenter))
            return;

        if (!ShouldDragWindow(parentPresenter)) return;

        if (MonitorReentry(e))
            return;

        var myWindow = this.GetWindow();

        if (myWindow == null)
            return;

        if (_interTabTransfer != null)
        {
            var cursorPos = e.DragDeltaEventArgs.PointerEventArgs.GetPosition(null);

            if (_interTabTransfer.BreachOrientation == Orientation.Vertical)
            {
                var vector = cursorPos - _interTabTransfer.DragStartWindowOffset;

                myWindow.Position = new PixelPoint((int) vector.X, (int) vector.Y);
            }
            else
            {
                var offset =
                    draggedItem.TranslatePoint(_interTabTransfer.OriginatorContainer.MouseAtDragStart, myWindow).Value;
                var borderVector = myWindow.PointToScreen(new Point()) - myWindow.Position;

                offset = new Point(offset.X + borderVector.X, offset.Y + borderVector.Y);
                myWindow.Position = PixelPoint.FromPoint(new Point(cursorPos.X - offset.X, cursorPos.Y - offset.Y), 1);
            }

            myWindow.Position = new PixelPoint((int) (myWindow.Position.X + e.DragDeltaEventArgs.Vector.X),
                (int) (myWindow.Position.Y + e.DragDeltaEventArgs.Vector.Y));
        }
        else
        {
            myWindow.Position = new PixelPoint((int) (myWindow.Position.X + e.DragDeltaEventArgs.Vector.X),
                (int) (myWindow.Position.Y + e.DragDeltaEventArgs.Vector.Y));
        }

        e.Handled = true;
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

        if (!Equals(_dragStartItem, draggedItem) || InterTabController == null) return;

        if (InterTabController?.InterTabClient == null)
            throw new InvalidOperationException("An InterTabClient must be provided on an InterTabController.");

        MonitorBreach(e);
    }

    private void ItemDragCompleted(object? sender, DragablzDragCompletedEventArgs e)
    {
        if (!IsMyItem(e.DragablzItem)) return;

        _interTabTransfer = null;
        ItemsPresenter.LockedMeasure = null;
        //IsDraggingWindow = false;
    }

    private void MonitorBreach(DragablzDragDeltaEventArgs e)
    {
        DragTabItem draggedItem = e.DragablzItem;
        PointerEventArgs pointerArgs = e.DragDeltaEventArgs.PointerEventArgs;

        var horizontalPopoutGrace = InterTabController.HorizontalPopoutGrace;
        var verticalPopoutGrace = InterTabController.VerticalPopoutGrace;

        var pointerPositionOnHeaderItemsControl = pointerArgs.GetPosition(ItemsPresenter);

        Orientation? breachOrientation = null;
        if (pointerPositionOnHeaderItemsControl.X < -horizontalPopoutGrace
            || (pointerPositionOnHeaderItemsControl.X - ItemsPresenter.Bounds.Width) > horizontalPopoutGrace)
            breachOrientation = Orientation.Horizontal;
        else if (pointerPositionOnHeaderItemsControl.Y < -verticalPopoutGrace
                 || (pointerPositionOnHeaderItemsControl.Y - ItemsPresenter.Bounds.Height) > verticalPopoutGrace)
            breachOrientation = Orientation.Vertical;

        if (!breachOrientation.HasValue)
            return;

        var newTabHost = InterTabController.InterTabClient.GetNewHost(InterTabController.InterTabClient,
            InterTabController.Partition, this);

        if (newTabHost?.TabablzControl == null || newTabHost.Container == null)
            throw new ApplicationException("New tab host was not correctly provided");

        var item = ItemsPresenter.ItemContainerGenerator.FindItem(draggedItem);
        var isTransposing = IsTransposing(newTabHost.TabablzControl);

        var myWindow = this.GetWindow();
        if (myWindow == null) throw new ApplicationException("Unable to find owning window.");

        var dragStartWindowOffset =
            ConfigureNewHostSizeAndGetDragStartWindowOffset(myWindow, newTabHost, draggedItem, isTransposing);

        var dragableItemHeaderPoint = draggedItem.TranslatePoint(new Point(), ItemsPresenter) ?? new Point();
        var dragableItemSize = draggedItem.Bounds;

        //var floatingItemSnapShots = this.VisualTreeDepthFirstTraversal()
        //    .OfType<Layout>()
        //    .SelectMany(l => l.FloatingDragablzItems().Select(FloatingItemSnapShot.Take))
        //    .ToList();

        var interTabTransfer = new InterTabTransfer(item, draggedItem, breachOrientation.Value, dragStartWindowOffset,
            draggedItem.MouseAtDragStart, dragableItemHeaderPoint, dragableItemSize, isTransposing);

        if (myWindow.WindowState == WindowState.Maximized)
        {
            //var desktopMousePosition = Native.GetCursorPos().ToWpf();
            //newTabHost.Container.Left = desktopMousePosition.X - dragStartWindowOffset.X;
            //newTabHost.Container.Top = desktopMousePosition.Y - dragStartWindowOffset.Y;
        }
        else
        {
            newTabHost.Container.Position = myWindow.Position;
        }

        newTabHost.Container.Show();

        //var contentPresenter = FindChildContentPresenter(item);

        ////stop the header shrinking if the tab stays open when empty
        //var minSize = EmptyHeaderSizingHint == EmptyHeaderSizingHint.PreviousTab
        //    ? new Size(_dragablzItemsControl.ActualWidth, _dragablzItemsControl.ActualHeight)
        //    : new Size();
        //System.Diagnostics.Debug.WriteLine("B " + minSize);

        RemoveFromSource(item);

        //_itemsHolder.Children.Remove(contentPresenter);

        //if (Items.Count == 0)
        //{
        //    _dragablzItemsControl.MinHeight = minSize.Height;
        //    _dragablzItemsControl.MinWidth = minSize.Width;
        //    Layout.ConsolidateBranch(this);
        //}

        RestorePreviousSelection();

        foreach (var dragablzItem in ItemsPresenter.DragablzItems())
        {
            dragablzItem.IsDragging = false;
            dragablzItem.IsSiblingDragging = false;
        }

        newTabHost.TabablzControl.ReceiveDrag(interTabTransfer, pointerArgs);

        //interTabTransfer.OriginatorContainer.IsDropTargetFound = true;

        e.Cancel = true;
    }

    private bool MonitorReentry(DragablzDragDeltaEventArgs e)
    {
        var draggedItem = e.DragablzItem;

        var sourceTabablzControl = TabsControlFromItemContainer(draggedItem);

        if (sourceTabablzControl.ItemsPresenter.DragablzItems().Count > 1 &&
            draggedItem.LogicalIndex < sourceTabablzControl.FixedHeaderCount)
            return false;

        var otherTabablzControls = LoadedInstances
            .Where(
                tc =>
                    tc != this && tc.InterTabController != null && InterTabController != null
                    && Equals(tc.InterTabController.Partition, InterTabController.Partition)
                    && tc.ItemsPresenter != null)
            .Select(tc =>
            {
                var topLeft = tc.ItemsPresenter.PointToScreen(new Point());
                var lastFixedItem = tc.ItemsPresenter.DragablzItems()
                    .OrderBy(di => di.LogicalIndex)
                    .Take(tc.ItemsPresenter.FixedItemCount)
                    .LastOrDefault();
                //TODO work this for vert tabs
                if (lastFixedItem != null)
                {
                    topLeft = new PixelPoint((int) (topLeft.X + lastFixedItem.X + lastFixedItem.Bounds.Width),
                        topLeft.Y);
                }


                var bottomRight =
                    tc.ItemsPresenter.PointToScreen(new Point(tc.ItemsPresenter.Bounds.Width,
                        tc.ItemsPresenter.Bounds.Height));

                return new {tc, topLeft, bottomRight};
            });

        var screenMousePosition =
            ItemsPresenter.PointToScreen(e.DragDeltaEventArgs.PointerEventArgs.GetPosition(ItemsPresenter));

        if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return false;


        //var target = Native.SortWindowsTopToBottom(desktop.Windows.OfType<Window>())
        var target = desktop.Windows.OfType<Window>()
            .Join(otherTabablzControls, w => w, a => a.tc.GetWindow(), (w, a) => a)
            .FirstOrDefault(a =>
                new Rect(a.topLeft.ToPoint(1), a.bottomRight.ToPoint(1)).Contains(screenMousePosition.ToPoint(1)));

        if (target == null)
            return false;

        //var mousePositionOnItem = Mouse.GetPosition(e.DragablzItem);
        var mousePositionOnItem = e.DragDeltaEventArgs.PointerEventArgs.GetPosition(draggedItem);

        //var floatingItemSnapShots = this.VisualTreeDepthFirstTraversal()
        //    .OfType<Layout>()
        //    .SelectMany(l => l.FloatingDragablzItems().Select(FloatingItemSnapShot.Take))
        //    .ToList();

        //e.DragablzItem.IsDropTargetFound = true;

        var item = RemoveItem(draggedItem);

        var interTabTransfer = new InterTabTransfer(item, draggedItem, mousePositionOnItem);
        draggedItem.IsDragging = false;

        target.tc.ReceiveDrag(interTabTransfer, e.DragDeltaEventArgs.PointerEventArgs);

        e.Cancel = true;
        return true;
    }

    private bool IsMyItem(DragTabItem item) => ItemsPresenter.DragablzItems().Contains(item);

    private bool IsTransposing(TabControl target) => IsVertical(this) != IsVertical(target);

    private Point ConfigureNewHostSizeAndGetDragStartWindowOffset(Window currentWindow, INewTabHost<Window> newTabHost,
        DragTabItem dragablzItem, bool isTransposing)
    {
        var layout = this.VisualTreeAncestory().OfType<Layout>().FirstOrDefault();
        Point dragStartWindowOffset;
        var relative = dragablzItem.TranslatePoint(new Point(), this);

        var restoreSize = Interaction.GetBehaviors(currentWindow).OfType<RestoreBoundsOnWindowBehavior>().First()
            .RestoreSize;

        if (layout != null)
        {
            newTabHost.Container.Width =
                Bounds.Width + Math.Max(0, restoreSize.Width - layout.Bounds.Width);
            newTabHost.Container.Height =
                Bounds.Height + Math.Max(0, restoreSize.Height - layout.Bounds.Height);
            dragStartWindowOffset = relative ?? new Point();
        }
        else
        {
            if (newTabHost.Container.GetType() == currentWindow.GetType())
            {
                newTabHost.Container.Width = restoreSize.Width;
                newTabHost.Container.Height = restoreSize.Height;
                dragStartWindowOffset = isTransposing
                    ? new Point(dragablzItem.MouseAtDragStart.X, dragablzItem.MouseAtDragStart.Y)
                    : dragablzItem.TranslatePoint(new Point(), currentWindow) ?? new Point();
            }
            else
            {
                newTabHost.Container.Width = Bounds.Width;
                newTabHost.Container.Height = Bounds.Height;

                dragStartWindowOffset = isTransposing ? new Point() : relative ?? new Point();
                dragStartWindowOffset = new Point(dragStartWindowOffset.X + dragablzItem.MouseAtDragStart.X,
                    dragStartWindowOffset.Y + dragablzItem.MouseAtDragStart.Y);
                return dragStartWindowOffset;
            }
        }

        dragStartWindowOffset = new Point(dragStartWindowOffset.X + dragablzItem.MouseAtDragStart.X,
            dragStartWindowOffset.Y + dragablzItem.MouseAtDragStart.Y);

        var borderVector = currentWindow.PointToScreen(new Point()) - currentWindow.Position;

        dragStartWindowOffset = new Point(dragStartWindowOffset.X + borderVector.X,
            dragStartWindowOffset.Y + borderVector.Y);

        return dragStartWindowOffset;
    }

    private void RestorePreviousSelection()
    {
        var previousSelection = _previousSelection?.Target;

        if (Items is not IList items)
            return;

        if (previousSelection != null && items.Contains(previousSelection))
            SelectedItem = previousSelection;
        else
            SelectedItem = Items.OfType<object>().FirstOrDefault();
    }

    private static bool IsVertical(TabControl tabControl) => tabControl.TabStripPlacement is Dock.Left or Dock.Right;

    private bool ShouldDragWindow(TabsItemsPresenter sourceOfDragItemsControl)
    {
        return sourceOfDragItemsControl.DragablzItems().Count == 1
               && (InterTabController == null || InterTabController.MoveWindowWithSolitaryTabs);
        //&& !Layout.IsContainedWithinBranch(sourceOfDragItemsControl));
    }

    #endregion
}