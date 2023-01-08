using static System.Math;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TabaloniaNew.Controls;


namespace TabaloniaNew.Panels
{
    public class TabsPanel : Panel
    {
        public TabsPanel()
        {
            Background = new SolidColorBrush(Color.Parse("#33ff0000"));
        }


        public double ItemWidth { get; internal set; }

        public double ItemOffset { get; internal set; }


        protected override Size MeasureOverride(Size availableSize)
        {
            var draggedItem = (DragTabItem?) Children.FirstOrDefault(c => c is DragTabItem
            {
                IsDragging: true
            });

            return draggedItem is not null 
                ? DragMeasureImpl(draggedItem, availableSize) 
                : MeasureImpl(availableSize);
        }
        

        protected override Size ArrangeOverride(Size finalSize)
        {
            double availableWidth = GetAvailableWidth(finalSize);

            double x = 0;
            int z = int.MaxValue;

            foreach (IControl? child in Children)
            {
                if (child is not DragTabItem tabItem)
                    continue;

                tabItem.ZIndex = tabItem.IsSelected ? int.MaxValue : --z;

                if (tabItem.IsDragging)
                {
                    SetLocation(tabItem, tabItem.X, availableWidth);
                }
                else
                {
                    SetLocation(tabItem, x, availableWidth);

                    x += availableWidth + ItemOffset;
                }
            }

            return finalSize;
        }
        

        private Size MeasureImpl(Size availableSize)
        {
            double availableWidth = GetAvailableWidth(availableSize);

            double height = 0;
            double width = 0;

            bool isFirst = true;

            foreach (var tabItem in Children)
            {
                tabItem.Measure(new Size(availableWidth, availableSize.Height));

                width += availableWidth;
                height = Max(tabItem.DesiredSize.Height, height);

                if (!isFirst)
                    width += ItemOffset;

                isFirst = false;
            }

            return new Size(width, height);
        }


        private Size DragMeasureImpl(DragTabItem draggedItem, Size availableSize)
        {
            double availableWidth = GetAvailableWidth(availableSize);

            double height = 0;
            double width = 0;

            bool isFirst = true;

            foreach (var tabItem in Children)
            {
                tabItem.Measure(new Size(availableWidth, availableSize.Height));
                
                width += availableWidth;
                height = Max(tabItem.DesiredSize.Height, height);

                if (!isFirst)
                    width += ItemOffset;

                isFirst = false;
            }
            
            if (draggedItem.X + availableWidth > width)
                return new Size(draggedItem.X + availableWidth, height);
            
            return new Size(width, height);
        }
        

        private double GetAvailableWidth(Size availableSize)
        {
            int tabsCount = Children.Count;

            if (tabsCount == 0)
                return 0;

            double itemWidth = availableSize.Width / tabsCount - ItemOffset * (tabsCount - 1) / tabsCount;

            return Min(ItemWidth, itemWidth);
        }


        private static void SetLocation(DragTabItem dragTabItem, double x, double width)
        {
            const double y = 0;

            dragTabItem.X = x;
            dragTabItem.Y = y;

            dragTabItem.Arrange(new Rect(new Point(x, y), new Size(width, dragTabItem.DesiredSize.Height)));
        }
    }
}