using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TabaloniaNew.Events;
using TabaloniaNew.Panels;


namespace TabaloniaNew.Controls
{
    public class TabsControl : TabControl
    {
        #region Private Fields

        private Thumb _rightDragWindowThumb;
        private TabsPanel _tabsPanel;
        private Thumb _leftDragWindowThumb;
        private DragTabItem? _draggedItem;

        #endregion
        
        
        #region Avalonia Properties
        
        public static readonly StyledProperty<bool> ShowDefaultCloseButtonProperty =
            AvaloniaProperty.Register<TabsControl, bool>(nameof(ShowDefaultCloseButton), defaultValue: true);
       
        
        public static readonly StyledProperty<Func<object>?> NewItemFactoryProperty =
            AvaloniaProperty.Register<TabsControl, Func<object>?>(nameof(NewItemFactory));
        
        #endregion

        
        #region Constructor

        public TabsControl()
        {
            AddHandler(DragTabItem.DragStarted, ItemDragStarted, handledEventsToo: true);
            AddHandler(DragTabItem.DragDelta, ItemDragDelta);
            AddHandler(DragTabItem.DragCompleted, ItemDragCompleted, handledEventsToo: true);

            ItemsPanel = new FuncTemplate<IPanel>(() =>
            {
                if (_tabsPanel is not null)
                    return _tabsPanel;
                
                _tabsPanel = new TabsPanel
                {
                    ItemWidth = 250,
                    ItemOffset = -8
                };
                
                return _tabsPanel;
            });
        }

        #endregion
        
        
        #region Public Properties
        
        public bool ShowDefaultCloseButton
        {
            get => GetValue(ShowDefaultCloseButtonProperty);
            set => SetValue(ShowDefaultCloseButtonProperty, value);
        }
        
        
        public Func<object>? NewItemFactory
        {
            get => GetValue(NewItemFactoryProperty);
            set => SetValue(NewItemFactoryProperty, value);
        }
        
        #endregion
        
        
        #region Public Methods
        
        public void CloseItem(object tabItemSource)
        {
            if (tabItemSource == null)
                throw new ApplicationException("Valid DragItem to close is required.");

            var tabItem = FindDragTabItem(tabItemSource);

            if (tabItem == null)
                return;
            
            RemoveItem(tabItem);
        }
        
        
        public void AddItem()
        {
            object? newItem = NewItemFactory?.Invoke();
            
            if (newItem == null) 
                throw new ApplicationException("NewItemFactory is null or returned null.");

            if (Items is IList itemsList)
                itemsList.Add(newItem);
            
            SelectedItem = newItem;

            //Dispatcher.UIThread.Post(ItemsPresenter.InvalidateMeasure, DispatcherPriority.Loaded);
        }
        
        #endregion
        
        
        #region Protected Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _leftDragWindowThumb = e.NameScope.Get<Thumb>("PART_LeftDragWindowThumb");
            _leftDragWindowThumb.DragDelta += WindowDragThumbOnDragDelta;
            _leftDragWindowThumb.DoubleTapped += WindowDragThumbOnDoubleTapped;
            
            _rightDragWindowThumb = e.NameScope.Get<Thumb>("PART_RightDragWindowThumb");
            _rightDragWindowThumb.DragDelta += WindowDragThumbOnDragDelta;
            _rightDragWindowThumb.DoubleTapped += WindowDragThumbOnDoubleTapped;
            
            e.NameScope.Get<TopPanel>("PART_TopPanel");
        }

        
        protected override IItemContainerGenerator CreateItemContainerGenerator() => new DragTabItemContainerGenerator(this);
        
        #endregion
        
        
        #region Private Methods
        
        private void RemoveItem(DragTabItem container)
        {
            var containerInfo =
                ItemContainerGenerator.Containers.FirstOrDefault(c => Equals(c.ContainerControl, container));

            if (containerInfo == null)
                return;

            object item = containerInfo.Item;
            
            if (Items is IList itemsList)
            {
                itemsList.Remove(item);

                if (itemsList.Count == 0)
                {
                    CloseThisWindow();
                }
            }
        }

        
        private void CloseThisWindow()
        {
            var window = this.LogicalTreeAncestory().OfType<Window>().FirstOrDefault();

            window?.Close();
        }


        private IReadOnlyList<DragTabItem> DragTabItems => ItemContainerGenerator.Containers<DragTabItem>().ToList();
        
        
        private void ItemDragStarted(object? sender, DragTabDragStartedEventArgs e)
        {
            _draggedItem = e.TabItem;
            
            SetDraggingItem(_draggedItem);
            
            e.Handled = true;
            
            _draggedItem.IsSelected = true;

            var itemInfo = ItemContainerGenerator.Containers.FirstOrDefault(c => Equals(c.ContainerControl, _draggedItem));

            if (itemInfo != null)
            {
                object item = itemInfo.Item;

                if (itemInfo.Item is TabItem tabItem)
                    tabItem.IsSelected = true;

                SelectedItem = item;
            }
        }
        

        private void ItemDragDelta(object? sender, DragTabDragDeltaEventArgs e)
        {
            _draggedItem.X += e.DragDeltaEventArgs.Vector.X;
            _draggedItem.Y += e.DragDeltaEventArgs.Vector.Y;
            
            Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
          
            e.Handled = true;
        }
        
        
        private void ItemDragCompleted(object? sender, DragTabDragCompletedEventArgs e)
        {
            foreach (var item in DragTabItems)
            {
                item.IsDragging = false;
                item.IsSiblingDragging = false;
            }
            
            _tabsPanel.LayoutUpdated += TabsPanelOnLayoutUpdated;
            
            Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
        }

        
        private void SetDraggingItem(DragTabItem draggedItem)
        {
            foreach (var item in DragTabItems)
            {
                item.IsDragging = false;
                item.IsSiblingDragging = true;
            }

            draggedItem.IsDragging = true;
            draggedItem.IsSiblingDragging = false;
        }
        
        
        private void TabsPanelOnLayoutUpdated(object? sender, EventArgs e)
        {
            _tabsPanel.LayoutUpdated -= TabsPanelOnLayoutUpdated;
            
            MoveTabModelsIfNeeded();

            _draggedItem = null;
        }


        private void MoveTabModelsIfNeeded()
        {
            var itemInfo = ItemContainerGenerator.Containers.FirstOrDefault(c => Equals(c.ContainerControl, _draggedItem));

            if (itemInfo != null)
            {
                object item = itemInfo.Item;
                DragTabItem container = (DragTabItem) itemInfo.ContainerControl;

                if (Items is IList list)
                {
                    if (container.LogicalIndex != list.IndexOf(item))
                    {
                        list.Remove(item);
                        list.Insert(container.LogicalIndex, item);
                        
                        SelectedItem = item;

                        for (int i = 0; i < DragTabItems.Count; i++)
                        {
                            DragTabItems[i].LogicalIndex = i;
                        }
                    }
                }
            }
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
        

        private void WindowDragThumbOnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            var window = this.LogicalTreeAncestory().OfType<Window>().FirstOrDefault();

            window?.RestoreWindow();
        }
    
        
        private void WindowDragThumbOnDragDelta(object? sender, VectorEventArgs e)
        {
            var window = this.LogicalTreeAncestory().OfType<Window>().FirstOrDefault();

            window?.DragWindow(e.Vector.X, e.Vector.Y);
        }
        
        #endregion
    }
}