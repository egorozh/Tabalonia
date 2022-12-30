using static System.Math;

using Avalonia;
using Avalonia.Controls;


namespace TabaloniaNew.Panels
{
    public class TabsPanel : Canvas
    {
        public double ItemWidth { get; set; } = 200;

        
        protected override Size MeasureOverride(Size availableSize)
        {
            double availableWidth = Min(ItemWidth, availableSize.Width / Children.Count);
           
            double height = 0;
            double width = 0;
            
            foreach (var tabItem in Children)
            {
                tabItem.Measure(new Size(availableWidth, availableSize.Height));

                width += availableWidth;
                height = Max(tabItem.DesiredSize.Height, height);
            }
            
            return new Size(width, height);
        }
        
        
        protected override Size ArrangeOverride(Size finalSize)
        {
            double availableWidth = Min(ItemWidth, finalSize.Width / Children.Count);
            
            double x = 0;
            double y = 0;
            
            foreach (var child in Children)
            {
                child.Arrange(new Rect(new Point(x, y), new Size(availableWidth, child.DesiredSize.Height)));

                x += availableWidth;
            }
            
            return finalSize;
        }
    }
}