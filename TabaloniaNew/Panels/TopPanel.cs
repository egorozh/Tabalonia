using Avalonia;
using Avalonia.Controls;

namespace TabaloniaNew.Panels
{
    public class TopPanel : StackPanel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double height = 0;
            double width = 0;
            
            if (Children.Count != 3)
                return new Size(width, height);
            
            Children[1].Measure(new Size(availableSize.Width, availableSize.Height));

            width += Children[1].DesiredSize.Width;
            
            Children[2].Measure(new Size(availableSize.Width - width, availableSize.Height));

            double dragThumbWidth = Children[2].DesiredSize.Width;

            width += dragThumbWidth;
            
            Children[0].Measure(new Size(availableSize.Width - width - dragThumbWidth, availableSize.Height));
            
            width += Children[0].DesiredSize.Width; 
            
            height = Math.Max(Children[0].DesiredSize.Height, height);
            
            return new Size(width, height);
        }
        
        
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count != 3)
                return finalSize;
            
            double minimumWidth = Children[1].DesiredSize.Width + Children[2].DesiredSize.Width;

            double tabsWidth = Children[0].DesiredSize.Width;
            double tabsHeight = Children[0].DesiredSize.Height;

            double x = 0;

            if (tabsWidth < finalSize.Width - minimumWidth)
            {
                Children[0].Arrange(new Rect(0, 0, tabsWidth, tabsHeight));
                Children[1].Arrange(new Rect(tabsWidth, 0, Children[1].DesiredSize.Width, Children[1].DesiredSize.Height));
                Children[2].Arrange(new Rect(tabsWidth + Children[1].DesiredSize.Width, 0, finalSize.Width - Children[0].DesiredSize.Width - Children[1].DesiredSize.Width, tabsHeight));
            }
            else
            {
                x = finalSize.Width - minimumWidth;
                
                Children[0].Arrange(new Rect(0, 0, x, tabsHeight));
                Children[1].Arrange(new Rect(x, 0, Children[1].DesiredSize.Width, Children[1].DesiredSize.Height));
                Children[2].Arrange(new Rect(x + Children[1].DesiredSize.Width, 0, Children[2].DesiredSize.Width, tabsHeight));
            }
            
            return finalSize;
        }
    }
}