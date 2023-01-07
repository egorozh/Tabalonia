using static System.Math;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;


namespace TabaloniaNew.Panels
{
    public class TabsPanel : Panel
    {
        
        public TabsPanel()
        {
            Background = new SolidColorBrush(Color.Parse("#33ff0000"));
        }

        
        public double ItemWidth { get; set; } = 200;

        public double ItemOffset { get; set; } = -8;
        
        
        protected override Size MeasureOverride(Size availableSize)
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
        

        protected override Size ArrangeOverride(Size finalSize)
        {
            double availableWidth = GetAvailableWidth(finalSize);
            
            double x = 0;
            int z = int.MaxValue;
            
            foreach (IControl? child in Children)
            {
                if (child is not TabItem tabItem)
                    continue;
                
                tabItem.ZIndex = tabItem.IsSelected ? int.MaxValue : --z;

                SetLocation(child, x, availableWidth);
                
                x += availableWidth + ItemOffset;
            }
            
            return finalSize;
        }
        
        
        private double GetAvailableWidth(Size availableSize)
        {
            int tabsCount = Children.Count;
            
            if (tabsCount == 0)
                return 0;

            double itemWidth = availableSize.Width / tabsCount - ItemOffset * (tabsCount - 1) / tabsCount;

            return Min(ItemWidth, itemWidth);
        }
        
        
        private static void SetLocation(ILayoutable control, double x, double width)
        {
            const double y = 0;

            control.Arrange(new Rect(new Point(x, y), new Size(width, control.DesiredSize.Height)));
        }
    }
}