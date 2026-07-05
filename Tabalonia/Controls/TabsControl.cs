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
    private bool _isDetachedHost;
    private bool _skipMoveTabModelsOnDragCompleted;
    private Point? _lastKnownDragScreenPoint;
    private TabsControl? _dragSessionSourceHost;
    private Window? _dragSessionWindow;
    private Vector? _dragSessionWindowPointerOffset;

    // Set when this control takes over pointer capture from the dragged tab's thumb, so the
    // drag session survives the tab container being destroyed on cross-host transfers.
    private IPointer? _dragSessionPointer;

    // Floating window kept alive (hidden) while its tab is attached to another strip, so the
    // session's pointer capture survives and the window can be reused on re-detach. Hiding does
    // not interrupt the drag: the OS keeps delivering the drag event stream to the window that
    // received the button press even after it is hidden.
    private Window? _hiddenSessionWindow;
    private TabsControl? _hiddenSessionHost;

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


    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!IsDragSessionPointerEvent(e))
            return;

        if (GetScreenPointFromEvent(e) is not { } screenPoint)
            return;

        _lastKnownDragScreenPoint = screenPoint;

        if (_dragSessionWindow is not null)
        {
            if (!TryAttachDuringDrag(screenPoint))
                MoveDragSessionWindow(screenPoint);
        }
        else if (_dragSessionSourceHost is { } host)
        {
            ContinueExternalDrag(host, screenPoint);
        }

        e.Handled = true;
    }


    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!IsDragSessionPointerEvent(e))
            return;

        e.Handled = true;
        FinalizeControllerDragSession(GetScreenPointFromEvent(e) ?? _lastKnownDragScreenPoint);
    }


    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (_dragSessionPointer is not null && ReferenceEquals(e.Pointer, _dragSessionPointer))
            FinalizeControllerDragSession(_lastKnownDragScreenPoint);

        base.OnPointerCaptureLost(e);
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

        if (IsDetachedSingleTabHost() && e.ScreenPoint is { } dragStartScreenPoint)
        {
            Window? hostWindow = GetThisWindow();

            if (hostWindow is not null)
            {
                _dragSessionWindow = hostWindow;
                _dragSessionWindowPointerOffset = new Vector(
                    x: dragStartScreenPoint.X - hostWindow.Position.X,
                    y: dragStartScreenPoint.Y - hostWindow.Position.Y);
            }
        }
    }


    private void ItemDragDelta(object? sender, DragTabDragDeltaEventArgs e)
    {
        if (_dragSessionPointer is not null || _draggedItem is null)
        {
            // The session is driven by this control's own pointer handlers (or already torn down);
            // thumb-originated deltas are stale.
            e.Handled = true;
            return;
        }

        if (_draggedItem.LogicalIndex < FixedHeaderCount)
        {
            e.Handled = true;
            return;
        }

        if (e.ScreenPoint is { } screenPoint)
        {
            _lastKnownDragScreenPoint = screenPoint;

            if (IsDetachedSingleTabHost() && _dragSessionWindow is not null)
            {
                if (!TryAttachDuringDrag(screenPoint))
                    MoveDragSessionWindow(screenPoint);

                e.Handled = true;
                return;
            }

            if (ReferenceEquals(_dragSessionSourceHost, this) && TryAttachOrDetachDuringDrag(screenPoint))
            {
                e.Handled = true;
                return;
            }
        }

        if (!ReferenceEquals(_dragSessionSourceHost, this))
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
        if (_dragSessionPointer is not null)
        {
            // Session is controller-driven; completion comes from OnPointerReleased/OnPointerCaptureLost.
            e.Handled = true;
            return;
        }

        Point? releaseScreenPoint = e.ScreenPoint ?? _lastKnownDragScreenPoint;
        TabsControl sourceHost = _dragSessionSourceHost ?? this;
        bool sourceDetachedDuringDrag = !ReferenceEquals(sourceHost, this);
        bool transferredBetweenHosts = TryTransferToAnotherHost(releaseScreenPoint, sourceHost);

        if (!transferredBetweenHosts && !sourceDetachedDuringDrag && !sourceHost.IsDetachedSingleTabHost())
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
            _skipMoveTabModelsOnDragCompleted = false;
        else
            MoveTabModelsIfNeeded();

        // While this control drives a cross-host drag session with its own pointer capture,
        // the dragged model reference must survive panel drag-completion cycles.
        if (_dragSessionPointer is not null)
            return;

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


    /// <summary>
    /// Called while the tab is still dragged inside its source strip. Once the pointer leaves the
    /// strip area, the tab is either attached directly to another strip under the pointer or
    /// detached into a floating window; either way the drag session continues.
    /// </summary>
    private bool TryAttachOrDetachDuringDrag(Point screenPoint)
    {
        if (_draggedTabModel is null)
            return false;

        if (!ShouldDetachForScreenPoint(screenPoint))
            return false;

        object model = _draggedTabModel;

        if (EnableTabAttaching &&
            TryFindDropTarget(screenPoint, this, out TabsControl? target) &&
            target is not null)
        {
            TakeOverPointerCapture();

            if (MoveItemToAnotherTabsControl(model, target, screenPoint))
            {
                EnterAttachedDragState(target, model, screenPoint);
                return true;
            }
        }

        if (!EnableTabDetaching)
            return false;

        TakeOverPointerCapture();

        if (!DetachItemToNewWindow(model, screenPoint, fromDragSession: true, out TabsControl? detachedHost, out Window? detachedWindow))
            return false;

        if (detachedHost is null || detachedWindow is null)
            return false;

        _dragSessionSourceHost = detachedHost;
        _dragSessionWindow = detachedWindow;
        _dragSessionWindowPointerOffset = new Vector(120, 20);
        _skipMoveTabModelsOnDragCompleted = true;

        ClearDragVisualFlags();

        detachedHost.MarkDraggedItemStateDeferred(model, screenPoint: null);

        return true;
    }


    /// <summary>
    /// Called while the tab floats in its own window. When the pointer moves over another strip,
    /// the tab is attached to it immediately and the floating window is hidden (kept alive so the
    /// pointer capture survives and the window can be reused if the tab is detached again).
    /// </summary>
    private bool TryAttachDuringDrag(Point screenPoint)
    {
        if (_draggedTabModel is null || !EnableTabAttaching)
            return false;

        TabsControl floatingHost = _dragSessionSourceHost ?? this;

        if (!floatingHost.EnableTabAttaching)
            return false;

        if (!TryFindDropTarget(screenPoint, floatingHost, out TabsControl? target) || target is null)
            return false;

        TakeOverPointerCapture();

        Window? floatingWindow = _dragSessionWindow;
        object model = _draggedTabModel;

        if (!floatingHost.MoveItemToAnotherTabsControl(model, target, screenPoint, suppressEmptySourceAction: true))
            return false;

        if (floatingWindow is not null)
        {
            _hiddenSessionWindow = floatingWindow;
            _hiddenSessionHost = floatingHost;
            floatingWindow.Hide();
        }

        if (!ReferenceEquals(floatingHost, this))
            floatingHost.EndExternalDragVisualState();

        EnterAttachedDragState(target, model, screenPoint);

        return true;
    }


    private void EnterAttachedDragState(TabsControl target, object model, Point screenPoint)
    {
        _dragSessionSourceHost = target;
        _dragSessionWindow = null;
        _dragSessionWindowPointerOffset = null;
        _skipMoveTabModelsOnDragCompleted = true;

        ClearDragVisualFlags();

        target.MarkDraggedItemStateDeferred(model, screenPoint);
    }


    /// <summary>
    /// Drives the drag inside the host currently holding the tab: moves the tab along the strip
    /// or detaches it again once the pointer leaves the strip area.
    /// </summary>
    private void ContinueExternalDrag(TabsControl host, Point screenPoint)
    {
        if (_draggedTabModel is null)
            return;

        if (host.EnableTabDetaching && host.ShouldDetachForScreenPoint(screenPoint))
        {
            RedetachFromHost(host, screenPoint);
            return;
        }

        host.DragExternalItemTo(screenPoint);
    }


    private void RedetachFromHost(TabsControl host, Point screenPoint)
    {
        object model = _draggedTabModel!;

        if (_hiddenSessionWindow is { } hiddenWindow && _hiddenSessionHost is { } hiddenHost)
        {
            if (!host.MoveItemToAnotherTabsControl(model, hiddenHost, screenPoint))
                return;

            hiddenWindow.Position = new PixelPoint(
                x: (int)Math.Round(screenPoint.X - 120),
                y: (int)Math.Round(screenPoint.Y - 20));
            hiddenWindow.Show();

            _dragSessionSourceHost = hiddenHost;
            _dragSessionWindow = hiddenWindow;
            _dragSessionWindowPointerOffset = new Vector(120, 20);
            _hiddenSessionWindow = null;
            _hiddenSessionHost = null;

            if (ReferenceEquals(host, this))
                ClearDragVisualFlags();
            else
                host.EndExternalDragVisualState();

            hiddenHost.MarkDraggedItemStateDeferred(model, screenPoint: null);
            return;
        }

        if (!host.DetachItemToNewWindow(model, screenPoint, fromDragSession: true, out TabsControl? newHost, out Window? newWindow))
            return;

        if (newHost is null || newWindow is null)
            return;

        _dragSessionSourceHost = newHost;
        _dragSessionWindow = newWindow;
        _dragSessionWindowPointerOffset = new Vector(120, 20);

        if (ReferenceEquals(host, this))
            ClearDragVisualFlags();
        else
            host.EndExternalDragVisualState();

        newHost.MarkDraggedItemStateDeferred(model, screenPoint: null);
    }


    private void TakeOverPointerCapture()
    {
        if (_dragSessionPointer is not null)
            return;

        if (_draggedItem?.HandOffDragToController() is not { } pointer)
            return;

        _dragSessionPointer = pointer;
        pointer.Capture(this);
    }


    private void ClearDragVisualFlags()
    {
        _dragging = false;

        foreach (var item in DragTabItems())
        {
            item.IsDragging = false;
            item.IsSiblingDragging = false;
        }
    }


    private bool IsDragSessionPointerEvent(PointerEventArgs e) =>
        _dragSessionPointer is not null && ReferenceEquals(e.Pointer, _dragSessionPointer);


    private Point? GetScreenPointFromEvent(PointerEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel)
            return null;

        PixelPoint screenPoint = topLevel.PointToScreen(e.GetPosition(topLevel));

        return new Point(screenPoint.X, screenPoint.Y);
    }


    private void FinalizeControllerDragSession(Point? releaseScreenPoint)
    {
        if (_dragSessionPointer is null)
            return;

        IPointer pointer = _dragSessionPointer;
        _dragSessionPointer = null;

        if (ReferenceEquals(pointer.Captured, this))
            pointer.Capture(null);

        TabsControl host = _dragSessionSourceHost ?? this;
        bool settlesInOwnStrip = _dragSessionWindow is null && ReferenceEquals(host, this);

        if (_dragSessionWindow is not null)
        {
            // Released while floating: the window stays as a regular detached window.
            if (ReferenceEquals(host, this))
                ClearDragVisualFlags();
            else
                host.EndExternalDragVisualState();
        }
        else
        {
            // Released while attached to a strip: let the host settle the tab and reorder its models.
            host.CompleteExternalDrag();
        }

        CloseHiddenSessionWindow();

        _dragging = false;
        _skipMoveTabModelsOnDragCompleted = false;

        if (!settlesInOwnStrip)
        {
            // When the tab settles in this control's own strip, the panel's drag-completion pass
            // still needs _draggedItem to reorder the models; TabsPanelOnDragCompleted clears it.
            _draggedItem = null;
            _draggedTabModel = null;
        }

        _lastKnownDragScreenPoint = null;
        ResetDragSession();

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
    }


    private void CloseHiddenSessionWindow()
    {
        if (_hiddenSessionWindow is { } hiddenWindow)
            Dispatcher.UIThread.Post(hiddenWindow.Close, DispatcherPriority.Background);

        _hiddenSessionWindow = null;
        _hiddenSessionHost = null;
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


    private bool IsDetachedSingleTabHost() =>
        _isDetachedHost && ItemsSource is IList items && items.Count == 1;


    private bool MoveItemToAnotherTabsControl(object item, TabsControl target, Point dropScreenPoint,
        bool suppressEmptySourceAction = false)
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

        if (suppressEmptySourceAction)
        {
            if (sourceItems.Count > 0 && removedItemWasSelected)
                SetSelectedNewTab(sourceItems, sourceIndex);
        }
        else
        {
            HandleSourceItemsChangedAfterTransfer(sourceItems, sourceIndex, removedItemWasSelected);
        }

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
            Margin = Margin,
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

        detachedTabsControl._isDetachedHost = true;


        detachedTabsControl.LastTabClosedAction = LastTabClosedAction == _defaultLastTabClosedAction
            ? detachedTabsControl._defaultLastTabClosedAction
            : LastTabClosedAction;

        return detachedTabsControl;
    }


    private void MarkDraggedItemStateDeferred(object? itemModel, Point? screenPoint) =>
        Dispatcher.UIThread.Post(() =>
        {
            MarkDraggedItemState(itemModel);

            if (screenPoint is { } point)
                DragExternalItemTo(point);
        }, DispatcherPriority.Loaded);


    /// <summary>
    /// Positions the dragged tab in this control's strip from a screen-space pointer location.
    /// Used when the drag session is driven by another <see cref="TabsControl"/>.
    /// </summary>
    private void DragExternalItemTo(Point screenPoint)
    {
        if (_draggedItem is null)
            return;

        if (TopLevel.GetTopLevel(_tabsPanel) is not { } topLevel)
            return;

        Point pointInTopLevel = topLevel.PointToClient(new PixelPoint(
            (int)Math.Round(screenPoint.X),
            (int)Math.Round(screenPoint.Y)));

        if (topLevel.TranslatePoint(pointInTopLevel, _tabsPanel) is not { } pointInPanel)
            return;

        double tabWidth = _draggedItem.Bounds.Width > 0 ? _draggedItem.Bounds.Width : TabItemWidth;

        _draggedItem.X = pointInPanel.X - tabWidth / 2;
        _draggedItem.Y = 0;

        _tabsPanel.InvalidateMeasure();
    }


    /// <summary>
    /// Finishes an externally driven drag: clears the drag visual state but keeps the dragged item
    /// reference so the panel's drag-completion pass reorders the models.
    /// </summary>
    private void CompleteExternalDrag()
    {
        foreach (var tabItem in DragTabItems())
        {
            tabItem.IsDragging = false;
            tabItem.IsSiblingDragging = false;
        }

        _dragging = false;

        Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
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
            WindowDecorations = sourceWindow?.WindowDecorations ?? WindowDecorations.Full,
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

            // The invisible floating window kept alive during an attached drag must not accept drops.
            if (ReferenceEquals(tabsControl, _hiddenSessionHost))
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