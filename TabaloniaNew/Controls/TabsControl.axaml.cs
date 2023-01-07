using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using TabaloniaNew.Events;
using TabaloniaNew.Panels;


namespace TabaloniaNew.Controls
{
    public class TabsControl : TabControl
    {
        private static readonly FuncTemplate<IPanel> DefaultPanel = new(() => new TabsPanel());
        private Thumb _dragWindowThumb;
    

        static TabsControl()
        {
            ItemsPanelProperty.OverrideDefaultValue<TabsControl>(DefaultPanel);
        }
        

        public TabsControl()
        {
            AddHandler(DragTabItem.DragStarted, ItemDragStarted, handledEventsToo: true);
        }
        
        
        #region Protected Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _dragWindowThumb = e.NameScope.Get<Thumb>("PART_DragWindowThumb");
            _dragWindowThumb.DragDelta += DragThumbOnDragDelta;
            _dragWindowThumb.DoubleTapped += DragThumbOnDoubleTapped;
        }

        
        protected override IItemContainerGenerator CreateItemContainerGenerator() => new DragTabItemContainerGenerator(this);
        
        #endregion
        
        
        
        
        private void ItemDragStarted(object? sender, DragTabDragStartedEventArgs e)
        {
            var draggedItem = e.TabItem;

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
        
        
        private void DragThumbOnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            var window = this.LogicalTreeAncestory().OfType<Window>().FirstOrDefault();

            window?.RestoreWindow();
        }
    
        
        private void DragThumbOnDragDelta(object? sender, VectorEventArgs e)
        {
            var window = this.LogicalTreeAncestory().OfType<Window>().FirstOrDefault();

            window?.DragWindow(e.Vector.X, e.Vector.Y);
        }
    }
}