using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tabalonia.Events;
using Tabalonia.Panels;


namespace Tabalonia.Controls;

public class TabsControl : TabControl
{
    #region Constants

    private const double DefaultTabWidth = 140;

    public const double WindowsAndLinuxDefaultLeftThumbWidth = 4d;
    public const double MacOsDefaultLeftThumbWidth = 80d;

    public const double WindowsDefaultRightThumbWidth = 160d;
    public const double MacOsDefaultRightThumbWidth = 50d;

    #endregion


    #region Private Fields

    private readonly TabsPanel _tabsPanel;

    private static readonly object RegistryLock = new();
    private static readonly List<WeakReference<TabsControl>> RegisteredTabsControls = new();

    private DragTabItem? _draggedItem;
    private object? _draggedTabModel;
    private bool _dragging;
    private bool _skipMoveTabModelsOnDragCompleted;
    private Point? _lastKnownDragScreenPoint;
    private TabsControl? _dragSessionSourceHost;
    private Window? _dragSessionWindow;
    private Vector? _dragSessionWindowPointerOffset;

    private Control? _topPanel;
    private readonly EventHandler<CloseLastTabEventArgs> _defaultLastTabClosedAction;

    private ICommand _addItemCommand;
    private ICommand _closeItemCommand;

    #endregion


    #region Avalonia Properties

    public static readonly StyledProperty<double> AdjacentHeaderItemOffsetProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(AdjacentHeaderItemOffset), defaultValue: 0);


    public static readonly StyledProperty<double> TabItemWidthProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(TabItemWidth), defaultValue: DefaultTabWidth);


    public static readonly StyledProperty<bool> ShowDefaultCloseButtonProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultCloseButton), defaultValue: true);


    public static readonly StyledProperty<bool> ShowDefaultAddButtonProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultAddButton), defaultValue: true);


    public static readonly StyledProperty<int> FixedHeaderCountProperty =
        AvaloniaProperty.Register<TabsControl, int>(nameof(FixedHeaderCount), defaultValue: 0);


    public static readonly StyledProperty<Func<Task<object>>?> NewItemAsyncFactoryProperty =
        AvaloniaProperty.Register<TabsControl, Func<Task<object>>?>(nameof(NewItemAsyncFactory));


    public static readonly StyledProperty<Func<object>?> NewItemFactoryProperty =
        AvaloniaProperty.Register<TabsControl, Func<object>?>(nameof(NewItemFactory));


    public static readonly StyledProperty<EventHandler<TabClosedEventArgs>?> TabClosedProperty =
        AvaloniaProperty.Register<TabsControl, EventHandler<TabClosedEventArgs>?>(nameof(TabClosed));

    public static readonly StyledProperty<EventHandler<TabClosingEventArgs>?> TabClosingProperty =
        AvaloniaProperty.Register<TabsControl, EventHandler<TabClosingEventArgs>?>(nameof(TabClosing));


    public static readonly StyledProperty<EventHandler<CloseLastTabEventArgs>?> LastTabClosedActionProperty =
        AvaloniaProperty.Register<TabsControl, EventHandler<CloseLastTabEventArgs>?>(nameof(LastTabClosedAction));


    public static readonly StyledProperty<double> LeftThumbWidthProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(LeftThumbWidth),
            defaultValue: OperatingSystem.IsMacOS()
                ? MacOsDefaultLeftThumbWidth
                : WindowsAndLinuxDefaultLeftThumbWidth);


    public static readonly StyledProperty<double> RightThumbWidthProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(RightThumbWidth),
            defaultValue: OperatingSystem.IsWindows() ? WindowsDefaultRightThumbWidth : MacOsDefaultRightThumbWidth);


    public static readonly DirectProperty<TabsControl, ICommand> AddItemCommandProperty =
        AvaloniaProperty.RegisterDirect<TabsControl, ICommand>(
            nameof(AddItemCommand),
            o => o.AddItemCommand,
            (o, v) => o.AddItemCommand = v);


    public static readonly DirectProperty<TabsControl, ICommand> CloseItemCommandProperty =
        AvaloniaProperty.RegisterDirect<TabsControl, ICommand>(
            nameof(CloseItemCommand),
            o => o.CloseItemCommand,
            (o, v) => o.CloseItemCommand = v);

    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<TabsControl, object?>(nameof(LeftContent));
    
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<TabsControl, object?>(nameof(RightContent));

    public static readonly StyledProperty<bool> EnableTabDetachingProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(EnableTabDetaching), defaultValue: true);

    public static readonly StyledProperty<bool> EnableTabAttachingProperty =
        AvaloniaProperty.Register<TabsControl, bool>(nameof(EnableTabAttaching), defaultValue: true);

    public static readonly StyledProperty<double> DetachTriggerDistanceProperty =
        AvaloniaProperty.Register<TabsControl, double>(nameof(DetachTriggerDistance), defaultValue: 32d);

    public static readonly StyledProperty<Func<TabsControl, Window>?> DetachedWindowFactoryProperty =
        AvaloniaProperty.Register<TabsControl, Func<TabsControl, Window>?>(nameof(DetachedWindowFactory));
    
    #endregion


    #region Constructor

    public TabsControl()
    {
        AddHandler(DragTabItem.DragStarted, ItemDragStarted, handledEventsToo: true);
        AddHandler(DragTabItem.DragDelta, ItemDragDelta);
        AddHandler(DragTabItem.DragCompleted, ItemDragCompleted, handledEventsToo: true);

        _tabsPanel = new TabsPanel(this)
        {
            ItemWidth = TabItemWidth,
            ItemOffset = AdjacentHeaderItemOffset
        };

        _tabsPanel.DragCompleted += TabsPanelOnDragCompleted;

        ItemsPanel = new FuncTemplate<Panel>(() => _tabsPanel);

        _defaultLastTabClosedAction = CreateDefaultLastTabClosedAction();
        LastTabClosedAction = _defaultLastTabClosedAction;

        _addItemCommand = new SimpleActionCommand(AddItem);
        _closeItemCommand = new SimpleParamActionCommand(CloseItem);
    }

    #endregion


    #region Public Properties
    
    public double AdjacentHeaderItemOffset
    {
        get => GetValue(AdjacentHeaderItemOffsetProperty);
        set => SetValue(AdjacentHeaderItemOffsetProperty, value);
    }


    public double TabItemWidth
    {
        get => GetValue(TabItemWidthProperty);
        set => SetValue(TabItemWidthProperty, value);
    }


    public bool ShowDefaultCloseButton
    {
        get => GetValue(ShowDefaultCloseButtonProperty);
        set => SetValue(ShowDefaultCloseButtonProperty, value);
    }


    public bool ShowDefaultAddButton
    {
        get => GetValue(ShowDefaultAddButtonProperty);
        set => SetValue(ShowDefaultAddButtonProperty, value);
    }


    public Func<Task<object>>? NewItemAsyncFactory
    {
        get => GetValue(NewItemAsyncFactoryProperty);
        set => SetValue(NewItemAsyncFactoryProperty, value);
    }


    public Func<object>? NewItemFactory
    {
        get => GetValue(NewItemFactoryProperty);
        set => SetValue(NewItemFactoryProperty, value);
    }


    public EventHandler<TabClosedEventArgs>? TabClosed
    {
        get => GetValue(TabClosedProperty);
        set => SetValue(TabClosedProperty, value);
    }


    public EventHandler<TabClosingEventArgs>? TabClosing
    {
        get => GetValue(TabClosingProperty);
        set => SetValue(TabClosingProperty, value);
    }


    public EventHandler<CloseLastTabEventArgs>? LastTabClosedAction
    {
        get => GetValue(LastTabClosedActionProperty);
        set => SetValue(LastTabClosedActionProperty, value);
    }


    /// <summary>
    /// Allows a the first adjacent tabs to be fixed (no dragging, and default close button will not show).
    /// </summary>
    public int FixedHeaderCount
    {
        get => GetValue(FixedHeaderCountProperty);
        set => SetValue(FixedHeaderCountProperty, value);
    }


    public double LeftThumbWidth
    {
        get => GetValue(LeftThumbWidthProperty);
        set => SetValue(LeftThumbWidthProperty, value);
    }


    public double RightThumbWidth
    {
        get => GetValue(RightThumbWidthProperty);
        set => SetValue(RightThumbWidthProperty, value);
    }


    public ICommand AddItemCommand
    {
        get => _addItemCommand;
        private set => SetAndRaise(AddItemCommandProperty, ref _addItemCommand, value);
    }


    public ICommand CloseItemCommand
    {
        get => _closeItemCommand;
        private set => SetAndRaise(CloseItemCommandProperty, ref _closeItemCommand, value);
    }

    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }
    
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    /// <summary>
    /// Enables creating a new host window when a tab drag ends outside of any registered tab strip.
    /// </summary>
    public bool EnableTabDetaching
    {
        get => GetValue(EnableTabDetachingProperty);
        set => SetValue(EnableTabDetachingProperty, value);
    }

    /// <summary>
    /// Enables dropping a dragged tab into another <see cref="TabsControl"/>.
    /// </summary>
    public bool EnableTabAttaching
    {
        get => GetValue(EnableTabAttachingProperty);
        set => SetValue(EnableTabAttachingProperty, value);
    }

    /// <summary>
    /// Screen-space tolerance around the tab strip where drag release is still treated as in-strip.
    /// </summary>
    public double DetachTriggerDistance
    {
        get => GetValue(DetachTriggerDistanceProperty);
        set => SetValue(DetachTriggerDistanceProperty, value);
    }

    /// <summary>
    /// Optional factory to create a host window for a detached tab.
    /// </summary>
    public Func<TabsControl, Window>? DetachedWindowFactory
    {
        get => GetValue(DetachedWindowFactoryProperty);
        set => SetValue(DetachedWindowFactoryProperty, value);
    }
    
    #endregion


    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _topPanel = e.NameScope.Get<Control>("PART_TopPanel");

        var leftDragWindowThumb = e.NameScope.Get<Thumb>("PART_LeftDragWindowThumb");
        leftDragWindowThumb.AddHandler(PointerPressedEvent, OnThumbBeginDrag, handledEventsToo: true);
        //leftDragWindowThumb.DragDelta += WindowDragThumbOnDragDelta;
        leftDragWindowThumb.DoubleTapped += WindowDragThumbOnDoubleTapped;

        var rightDragWindowThumb = e.NameScope.Get<Thumb>("PART_RightDragWindowThumb");
        rightDragWindowThumb.AddHandler(PointerPressedEvent, OnThumbBeginDrag, handledEventsToo: true);
        // rightDragWindowThumb.DragDelta += WindowDragThumbOnDragDelta;
        rightDragWindowThumb.DoubleTapped += WindowDragThumbOnDoubleTapped;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        new DragTabItem();


    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegisterTabsControl(this);
    }


    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnregisterTabsControl(this);
        base.OnDetachedFromVisualTree(e);
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AdjacentHeaderItemOffsetProperty)
        {
            _tabsPanel.ItemOffset = AdjacentHeaderItemOffset;
        }
        else if (change.Property == TabItemWidthProperty)
        {
            _tabsPanel.ItemWidth = TabItemWidth;
        }
    }

    #endregion


    #region Private Methods

    private void RemoveItem(DragTabItem container)
    {
        object? item = ItemFromContainer(container);

        if (item == null)
            return;

        if (ItemsSource is not IList itemsList)
            return;

        int removedItemIndex = itemsList.IndexOf(item);

        if (removedItemIndex == -1)
            return;

        TabClosingEventArgs tabClosingEventArgs = new(item);
        TabClosing?.Invoke(this, tabClosingEventArgs);
        if (tabClosingEventArgs.Cancel)
            return;

        bool removedItemIsSelected = SelectedItem == item;

        itemsList.Remove(item);

        TabClosed?.Invoke(this, new TabClosedEventArgs(item));

        if (itemsList.Count == 0)
            LastTabClosedAction?.Invoke(this, new CloseLastTabEventArgs(GetThisWindow()));
        else if (removedItemIsSelected)
            SetSelectedNewTab(itemsList, removedItemIndex);
    }


    private void SetSelectedNewTab(IList items, int removedItemIndex) =>
        SelectedItem = removedItemIndex == items.Count ? items[^1] : items[removedItemIndex];


    private Window? GetThisWindow() => this.FindLogicalAncestorOfType<Window>();


    private IEnumerable<DragTabItem> DragTabItems()
    {
        foreach (object item in Items)
        {
            var container = ContainerFromItem(item);

            if (container is DragTabItem dragTabItem)
                yield return dragTabItem;
        }
    }


    private void ItemDragStarted(object? sender, DragTabDragStartedEventArgs e)
    {
        _draggedItem = e.TabItem;
        _draggedTabModel = ItemFromContainer(_draggedItem);
        _lastKnownDragScreenPoint = e.ScreenPoint;
        _dragSessionSourceHost = this;
        _skipMoveTabModelsOnDragCompleted = false;

        e.Handled = true;

        _draggedItem.IsSelected = true;

        object? item = ItemFromContainer(_draggedItem);

        if (item != null)
        {
            if (item is TabItem tabItem)
                tabItem.IsSelected = true;

            SelectedItem = item;
        }
    }


    private void ItemDragDelta(object? sender, DragTabDragDeltaEventArgs e)
    {
        if (_draggedItem is null)
            throw new Exception($"{nameof(TabsControl)}.{nameof(ItemDragDelta)} - _draggedItem is null");

        if (e.ScreenPoint is { } screenPoint)
        {
            _lastKnownDragScreenPoint = screenPoint;

            if (ReferenceEquals(_dragSessionSourceHost, this))
                TryDetachDuringDrag(screenPoint);

            MoveDragSessionWindow(screenPoint);
        }

        if (!ReferenceEquals(_dragSessionSourceHost, this))
        {
            e.Handled = true;
            return;
        }

        if (_draggedItem.LogicalIndex < FixedHeaderCount)
        {
            e.Handled = true;
            return;
        }

        if (!_dragging)
        {
            _dragging = true;
            SetDraggingItem(_draggedItem);
        }

        _draggedItem.X += e.DragDeltaEventArgs.Vector.X;
        _draggedItem.Y += e.DragDeltaEventArgs.Vector.Y;

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);

        e.Handled = true;
    }


    private void ItemDragCompleted(object? sender, DragTabDragCompletedEventArgs e)
    {
        Point? releaseScreenPoint = e.ScreenPoint ?? _lastKnownDragScreenPoint;
        TabsControl sourceHost = _dragSessionSourceHost ?? this;
        bool sourceDetachedDuringDrag = !ReferenceEquals(sourceHost, this);
        bool transferredBetweenHosts = TryTransferToAnotherHost(releaseScreenPoint, sourceHost);

        if (!transferredBetweenHosts && !sourceDetachedDuringDrag)
            transferredBetweenHosts = TryDetachToNewWindow(releaseScreenPoint);

        if (!transferredBetweenHosts && sourceDetachedDuringDrag)
            transferredBetweenHosts = true;

        _skipMoveTabModelsOnDragCompleted = transferredBetweenHosts;

        foreach (var item in DragTabItems())
        {
            item.IsDragging = false;
            item.IsSiblingDragging = false;
        }

        if (sourceDetachedDuringDrag)
            sourceHost.EndExternalDragVisualState();

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);

        _dragging = false;
        _lastKnownDragScreenPoint = null;
        ResetDragSession();
    }


    private void SetDraggingItem(DragTabItem draggedItem)
    {
        foreach (var item in DragTabItems())
        {
            item.IsDragging = false;
            item.IsSiblingDragging = true;
        }

        draggedItem.IsDragging = true;
        draggedItem.IsSiblingDragging = false;
    }


    private void TabsPanelOnDragCompleted()
    {
        if (_skipMoveTabModelsOnDragCompleted)
        {
            _skipMoveTabModelsOnDragCompleted = false;
            _draggedItem = null;
            _draggedTabModel = null;
            _lastKnownDragScreenPoint = null;
            ResetDragSession();
            return;
        }

        MoveTabModelsIfNeeded();

        _draggedItem = null;
        _draggedTabModel = null;
        _lastKnownDragScreenPoint = null;
        ResetDragSession();
    }


    private void MoveTabModelsIfNeeded()
    {
        if (_draggedItem is null)
            return;

        object? item = ItemFromContainer(_draggedItem);

        if (item != null)
        {
            DragTabItem container = _draggedItem;

            if (ItemsSource is IList list)
            {
                if (container.LogicalIndex != list.IndexOf(item))
                {
                    list.Remove(item);
                    list.Insert(container.LogicalIndex, item);

                    SelectedItem = item;

                    int i = 0;

                    foreach (var dragTabItem in DragTabItems())
                        dragTabItem.LogicalIndex = i++;
                }
            }
        }
    }


    private bool TryTransferToAnotherHost(Point? releaseScreenPoint, TabsControl sourceHost)
    {
        if (!EnableTabAttaching || !sourceHost.EnableTabAttaching || releaseScreenPoint is null)
            return false;

        if (!TryFindDropTarget(releaseScreenPoint.Value, sourceHost, out TabsControl? target))
            return false;

        if (_draggedTabModel is null)
            return false;

        if (target is null)
            return false;

        return sourceHost.MoveItemToAnotherTabsControl(_draggedTabModel, target, releaseScreenPoint.Value);
    }


    private bool TryDetachToNewWindow(Point? releaseScreenPoint)
    {
        if (!EnableTabDetaching || releaseScreenPoint is null)
            return false;

        if (!ShouldDetachForScreenPoint(releaseScreenPoint.Value))
            return false;

        if (_draggedTabModel is null)
            return false;

        return DetachItemToNewWindow(_draggedTabModel, releaseScreenPoint.Value);
    }


    private void TryDetachDuringDrag(Point screenPoint)
    {
        if (_draggedTabModel is null || !EnableTabDetaching)
            return;

        if (!ShouldDetachForScreenPoint(screenPoint))
            return;

        if (DetachItemToNewWindow(_draggedTabModel, screenPoint, fromDragSession: true, out TabsControl? detachedHost, out Window? detachedWindow))
        {
            if (detachedHost is null || detachedWindow is null)
                return;

            _dragSessionSourceHost = detachedHost;
            _dragSessionWindow = detachedWindow;
            _dragSessionWindowPointerOffset = new Vector(120, 20);
            _skipMoveTabModelsOnDragCompleted = true;
            _dragging = false;

            foreach (var item in DragTabItems())
            {
                item.IsDragging = false;
                item.IsSiblingDragging = false;
            }

            Dispatcher.UIThread.Post(() => detachedHost.MarkDraggedItemState(_draggedTabModel), DispatcherPriority.Loaded);
        }
    }


    private void MoveDragSessionWindow(Point screenPoint)
    {
        if (_dragSessionWindow is null)
            return;

        Vector pointerOffset = _dragSessionWindowPointerOffset ?? new Vector(120, 20);
        _dragSessionWindow.Position = new PixelPoint(
            x: (int)Math.Round(screenPoint.X - pointerOffset.X),
            y: (int)Math.Round(screenPoint.Y - pointerOffset.Y));
    }


    private void ResetDragSession()
    {
        _dragSessionSourceHost = null;
        _dragSessionWindow = null;
        _dragSessionWindowPointerOffset = null;
    }


    private bool MoveItemToAnotherTabsControl(object item, TabsControl target, Point dropScreenPoint)
    {
        if (ReferenceEquals(target, this))
            return false;

        if (ItemsSource is not IList sourceItems || target.ItemsSource is not IList targetItems)
            return false;

        int sourceIndex = sourceItems.IndexOf(item);

        if (sourceIndex < 0)
            return false;

        bool removedItemWasSelected = Equals(SelectedItem, item);
        int targetInsertIndex = target.ResolveInsertIndexFromScreenPoint(dropScreenPoint);

        sourceItems.RemoveAt(sourceIndex);

        targetItems.Insert(targetInsertIndex, item);
        target.SelectedItem = item;

        HandleSourceItemsChangedAfterTransfer(sourceItems, sourceIndex, removedItemWasSelected);

        return true;
    }


    private int ResolveInsertIndexFromScreenPoint(Point screenPoint)
    {
        if (ItemsSource is not IList targetItems)
            return 0;

        int maxIndex = targetItems.Count;
        int minIndex = Math.Min(FixedHeaderCount, maxIndex);

        var orderedTabs = DragTabItems()
            .OrderBy(tab => tab.LogicalIndex)
            .ToList();

        if (orderedTabs.Count == 0)
            return minIndex;

        double pointerPosition = GetPointerPrimaryAxis(screenPoint);

        foreach (DragTabItem tab in orderedTabs)
        {
            if (tab.LogicalIndex < FixedHeaderCount)
                continue;

            if (!TryGetTabBoundsInScreen(tab, out Rect tabBounds))
                continue;

            if (pointerPosition < GetRectMidpoint(tabBounds))
                return Math.Clamp(tab.LogicalIndex, minIndex, maxIndex);
        }

        return maxIndex;
    }


    private double GetPointerPrimaryAxis(Point screenPoint) =>
        TabStripPlacement is Dock.Left or Dock.Right ? screenPoint.Y : screenPoint.X;


    private double GetRectMidpoint(Rect rect) =>
        TabStripPlacement is Dock.Left or Dock.Right
            ? rect.Y + rect.Height / 2
            : rect.X + rect.Width / 2;


    private static bool TryGetTabBoundsInScreen(Control tab, out Rect bounds)
    {
        bounds = default;

        TopLevel? topLevel = TopLevel.GetTopLevel(tab);

        if (topLevel is null)
            return false;

        Point? topLeftInTopLevel = tab.TranslatePoint(new Point(0, 0), topLevel);

        if (topLeftInTopLevel is null)
            return false;

        PixelPoint topLeftScreen = topLevel.PointToScreen(topLeftInTopLevel.Value);
        PixelPoint bottomRightScreen = topLevel.PointToScreen(topLeftInTopLevel.Value + new Vector(tab.Bounds.Width, tab.Bounds.Height));

        bounds = new Rect(
            x: topLeftScreen.X,
            y: topLeftScreen.Y,
            width: Math.Max(1, bottomRightScreen.X - topLeftScreen.X),
            height: Math.Max(1, bottomRightScreen.Y - topLeftScreen.Y));

        return true;
    }


    private bool DetachItemToNewWindow(object item, Point releaseScreenPoint) =>
        DetachItemToNewWindow(item, releaseScreenPoint, fromDragSession: false, out _, out _);


    private bool DetachItemToNewWindow(
        object item,
        Point releaseScreenPoint,
        bool fromDragSession,
        out TabsControl? detachedTabsControl,
        out Window? detachedWindow)
    {
        detachedTabsControl = null;
        detachedWindow = null;

        if (ItemsSource is not IList sourceItems)
            return false;

        int sourceIndex = sourceItems.IndexOf(item);

        if (sourceIndex < 0)
            return false;

        detachedTabsControl = CreateDetachedTabsControl();

        if (detachedTabsControl.ItemsSource is not IList detachedItems)
            return false;

        bool removedItemWasSelected = Equals(SelectedItem, item);

        sourceItems.RemoveAt(sourceIndex);

        detachedItems.Add(item);
        detachedTabsControl.SelectedItem = item;

        detachedWindow = DetachedWindowFactory?.Invoke(detachedTabsControl)
                         ?? CreateDefaultDetachedWindow(detachedTabsControl);

        detachedWindow.Position = new PixelPoint(
            x: (int)releaseScreenPoint.X - 120,
            y: (int)releaseScreenPoint.Y - 20);

        detachedWindow.Show();

        if (fromDragSession)
            detachedWindow.Activate();

        HandleSourceItemsChangedAfterTransfer(sourceItems, sourceIndex, removedItemWasSelected);

        return true;
    }


    private TabsControl CreateDetachedTabsControl()
    {
        var detachedTabsControl = new TabsControl
        {
            AdjacentHeaderItemOffset = AdjacentHeaderItemOffset,
            TabItemWidth = TabItemWidth,
            ShowDefaultCloseButton = ShowDefaultCloseButton,
            ShowDefaultAddButton = ShowDefaultAddButton,
            FixedHeaderCount = 0,
            NewItemAsyncFactory = NewItemAsyncFactory,
            NewItemFactory = NewItemFactory,
            TabClosed = TabClosed,
            TabClosing = TabClosing,
            LeftThumbWidth = LeftThumbWidth,
            RightThumbWidth = RightThumbWidth,
            EnableTabDetaching = EnableTabDetaching,
            EnableTabAttaching = EnableTabAttaching,
            DetachTriggerDistance = DetachTriggerDistance,
            DetachedWindowFactory = DetachedWindowFactory,
            ItemTemplate = ItemTemplate,
            ContentTemplate = ContentTemplate,
            ItemsSource = new ObservableCollection<object?>(),
            DataContext = DataContext
        };


        detachedTabsControl.LastTabClosedAction = LastTabClosedAction == _defaultLastTabClosedAction
            ? detachedTabsControl._defaultLastTabClosedAction
            : LastTabClosedAction;

        return detachedTabsControl;
    }


    private void MarkDraggedItemState(object? itemModel)
    {
        if (itemModel is null)
            return;

        foreach (var tabItem in DragTabItems())
        {
            tabItem.IsDragging = false;
            tabItem.IsSiblingDragging = true;
        }

        if (ContainerFromItem(itemModel) is DragTabItem draggedItem)
        {
            draggedItem.IsDragging = true;
            draggedItem.IsSiblingDragging = false;
            draggedItem.IsSelected = true;
            _draggedItem = draggedItem;
            _draggedTabModel = itemModel;
            _dragging = true;
        }

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
    }


    private void EndExternalDragVisualState()
    {
        foreach (var tabItem in DragTabItems())
        {
            tabItem.IsDragging = false;
            tabItem.IsSiblingDragging = false;
        }

        _dragging = false;
        _draggedItem = null;
        _draggedTabModel = null;

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
    }


    private void HandleSourceItemsChangedAfterTransfer(IList sourceItems, int removedItemIndex, bool removedItemWasSelected)
    {
        if (sourceItems.Count == 0)
        {
            Dispatcher.UIThread.Post(
                () => LastTabClosedAction?.Invoke(this, new CloseLastTabEventArgs(GetThisWindow())),
                DispatcherPriority.Background);

            return;
        }

        if (removedItemWasSelected)
            SetSelectedNewTab(sourceItems, removedItemIndex);
    }


    private static EventHandler<CloseLastTabEventArgs> CreateDefaultLastTabClosedAction() =>
        (_, args) => args.Window?.Close();


    private Window CreateDefaultDetachedWindow(TabsControl detachedTabsControl)
    {
        Window? sourceWindow = GetThisWindow();

        var detachedWindow = new Window
        {
            Width = sourceWindow?.Bounds.Width is > 0 ? sourceWindow.Bounds.Width : 900,
            Height = sourceWindow?.Bounds.Height is > 0 ? sourceWindow.Bounds.Height : 600,
            MinWidth = sourceWindow?.MinWidth ?? 320,
            MinHeight = sourceWindow?.MinHeight ?? 240,
            Background = sourceWindow?.Background,
            SystemDecorations = sourceWindow?.SystemDecorations ?? SystemDecorations.Full,
            ExtendClientAreaToDecorationsHint = sourceWindow?.ExtendClientAreaToDecorationsHint ?? false,
            Content = detachedTabsControl,
            DataContext = detachedTabsControl.DataContext,
            Title = sourceWindow?.Title ?? "Detached Tab"
        };

        if (sourceWindow?.Icon is { } sourceIcon)
            detachedWindow.Icon = sourceIcon;

        return detachedWindow;
    }


    private bool ShouldDetachForScreenPoint(Point screenPoint)
    {
        if (!TryGetDropBoundsInScreen(out Rect bounds))
            return false;

        var expandedBounds = bounds.Inflate(DetachTriggerDistance);

        return !expandedBounds.Contains(screenPoint);
    }


    private bool TryFindDropTarget(Point screenPoint, TabsControl? excludedControl, out TabsControl? target)
    {
        target = null;

        foreach (TabsControl tabsControl in EnumerateRegisteredTabsControls())
        {
            if (ReferenceEquals(tabsControl, excludedControl))
                continue;

            if (!tabsControl.EnableTabAttaching)
                continue;

            if (!tabsControl.TryGetDropBoundsInScreen(out Rect bounds))
                continue;

            if (bounds.Contains(screenPoint))
            {
                target = tabsControl;
                return true;
            }
        }

        return false;
    }


    private bool TryGetDropBoundsInScreen(out Rect bounds)
    {
        bounds = default;

        if (_topPanel is null)
            return false;

        TopLevel? topLevel = TopLevel.GetTopLevel(_topPanel);

        if (topLevel is null)
            return false;

        Point? topLeftInTopLevel = _topPanel.TranslatePoint(new Point(0, 0), topLevel);

        if (topLeftInTopLevel is null)
            return false;

        PixelPoint topLeftScreen = topLevel.PointToScreen(topLeftInTopLevel.Value);
        PixelPoint bottomRightScreen = topLevel.PointToScreen(topLeftInTopLevel.Value + new Vector(_topPanel.Bounds.Width, _topPanel.Bounds.Height));

        bounds = new Rect(
            x: topLeftScreen.X,
            y: topLeftScreen.Y,
            width: Math.Max(1, bottomRightScreen.X - topLeftScreen.X),
            height: Math.Max(1, bottomRightScreen.Y - topLeftScreen.Y));

        return true;
    }


    private static IEnumerable<TabsControl> EnumerateRegisteredTabsControls()
    {
        List<TabsControl> aliveControls = new();

        lock (RegistryLock)
        {
            for (int i = RegisteredTabsControls.Count - 1; i >= 0; i--)
            {
                if (RegisteredTabsControls[i].TryGetTarget(out TabsControl? tabsControl))
                {
                    aliveControls.Add(tabsControl);
                }
                else
                {
                    RegisteredTabsControls.RemoveAt(i);
                }
            }
        }

        return aliveControls;
    }


    private static void RegisterTabsControl(TabsControl tabsControl)
    {
        lock (RegistryLock)
        {
            bool alreadyRegistered = RegisteredTabsControls.Any(reference =>
                reference.TryGetTarget(out TabsControl? existing) && ReferenceEquals(existing, tabsControl));

            if (!alreadyRegistered)
                RegisteredTabsControls.Add(new WeakReference<TabsControl>(tabsControl));
        }
    }


    private static void UnregisterTabsControl(TabsControl tabsControl)
    {
        lock (RegistryLock)
        {
            for (int i = RegisteredTabsControls.Count - 1; i >= 0; i--)
            {
                if (!RegisteredTabsControls[i].TryGetTarget(out TabsControl? existing) || ReferenceEquals(existing, tabsControl))
                    RegisteredTabsControls.RemoveAt(i);
            }
        }
    }

    private void OnThumbBeginDrag(object? sender, PointerPressedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        if(toplevel is not Window window) return;
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
            e.GetCurrentPoint(this).Pointer.Type == PointerType.Touch)
        {
            window.BeginMoveDrag(e);
        }
    }

    private void WindowDragThumbOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = this.FindLogicalAncestorOfType<Window>();

        window?.RestoreWindow();
    }

    [Obsolete]
    private void WindowDragThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        var window = this.FindLogicalAncestorOfType<Window>();

        window?.DragWindow(e.Vector.X, e.Vector.Y);
    }


    private void AddItem()
    {
        if (NewItemAsyncFactory is not null)
        {
            NewItemAsyncFactory.Invoke().ContinueWith(t => { AddItem(t.Result); },
                scheduler: TaskScheduler.FromCurrentSynchronizationContext());

            return;
        }

        AddItem(NewItemFactory?.Invoke());
    }


    private void AddItem(object? newItem)
    {
        ArgumentNullException.ThrowIfNull(newItem);

        if (ItemsSource is IList itemsList)
            itemsList.Add(newItem);

        SelectedItem = newItem;
    }


    private void CloseItem(object? tabItemSource)
    {
        ArgumentNullException.ThrowIfNull(tabItemSource);

        if (tabItemSource is not DragTabItem tabItem)
            return;

        RemoveItem(tabItem);
    }

    #endregion
}