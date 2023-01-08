using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using TabaloniaNew.Events;
using TabaloniaNew.Panels;


namespace TabaloniaNew.Controls
{
    public class TabsControl : TabControl
    {
        #region Private Fields

        private Thumb _rightDragWindowThumb;
        private TabsPanel _tabsPanel;
        private TopPanel _topPanel;
        private Thumb _leftDragWindowThumb;

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
                    ItemWidth = 200,
                    ItemOffset = -8
                };
                
                return _tabsPanel;
            });
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
            
            _topPanel = e.NameScope.Get<TopPanel>("PART_TopPanel");
        }

        
        protected override IItemContainerGenerator CreateItemContainerGenerator() => new DragTabItemContainerGenerator(this);
        
        #endregion
        
        
        #region Private Methods

        private IReadOnlyList<DragTabItem> DragTabItems => ItemContainerGenerator.Containers<DragTabItem>().ToList();
        
        
        private void ItemDragStarted(object? sender, DragTabDragStartedEventArgs e)
        {
            var draggedItem = e.TabItem;
            
            SetDraggingItem(draggedItem);
            
            e.Handled = true;
            
            draggedItem.IsSelected = true;

            var itemInfo = ItemContainerGenerator.Containers.FirstOrDefault(c => Equals(c.ContainerControl, draggedItem));

            if (itemInfo != null)
            {
                object item = itemInfo.Item;

                if (itemInfo.Item is TabItem tabItem)
                    tabItem.IsSelected = true;

                SelectedItem = item;
            }
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


        private void ItemDragDelta(object? sender, DragTabDragDeltaEventArgs e)
        {
            DragTabItem currentItem = e.TabItem;

            currentItem.X += e.DragDeltaEventArgs.Vector.X;
            currentItem.Y += e.DragDeltaEventArgs.Vector.Y;
            
            // desiredLocation =
            //     ItemsOrganiser.ConstrainLocation(this, this.Bounds, desiredLocation, currentItem, AddButton, _dragThumb);
            //
            // var siblingsItems = DragTabItems.Except(new[] {currentItem}).ToList();
            //
            // foreach (var dragableItem in siblingsItems)
            //     dragableItem.IsSiblingDragging = true;
            //
            // currentItem.IsDragging = true;

            // ItemsOrganiser.OrganiseOnDrag(siblingsItems, currentItem, AddButton, _dragThumb);

            Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
          
            e.Handled = true;
        }
        
        
        private void ItemDragCompleted(object? sender, DragablzDragCompletedEventArgs e)
        {
            foreach (var item in DragTabItems)
            {
                item.IsDragging = false;
                item.IsSiblingDragging = false;
            }
            
            Dispatcher.UIThread.Post(() => _tabsPanel.InvalidateMeasure(), DispatcherPriority.Loaded);
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