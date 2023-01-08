using Avalonia;
using Avalonia.Controls;


namespace TabaloniaNew.Panels;


public class TopPanel : StackPanel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double height = 0;
        double width = 0;
            
        if (Children.Count != 3)
            return new Size(width, height);
            
        var tabsControl = Children[0];
        var addTabButton = Children[1];
        var dragWindowThumb = Children[2];
            
        addTabButton.Measure(new Size(availableSize.Width, availableSize.Height));

        width += addTabButton.DesiredSize.Width;
            
        dragWindowThumb.Measure(new Size(availableSize.Width - width, availableSize.Height));

        double dragThumbWidth = dragWindowThumb.DesiredSize.Width;

        width += dragThumbWidth;
            
        tabsControl.Measure(new Size(availableSize.Width - width - dragThumbWidth, availableSize.Height));
            
        width += tabsControl.DesiredSize.Width; 
            
        height = Math.Max(tabsControl.DesiredSize.Height, height);
            
        return new Size(width, height);
    }
        
        
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count != 3)
            return finalSize;

        var tabsControl = Children[0];
        var addTabButton = Children[1];
        var dragWindowThumb = Children[2];
            
        double minimumWidth = addTabButton.DesiredSize.Width + dragWindowThumb.DesiredSize.Width;

        double tabsWidth = tabsControl.DesiredSize.Width;
        double tabsHeight = tabsControl.DesiredSize.Height;

        if (tabsWidth < finalSize.Width - minimumWidth)
        {
            tabsControl.Arrange(new Rect(0, 0, tabsWidth, tabsHeight));
            addTabButton.Arrange(new Rect(tabsWidth, 0, addTabButton.DesiredSize.Width, addTabButton.DesiredSize.Height));
            dragWindowThumb.Arrange(new Rect(tabsWidth + addTabButton.DesiredSize.Width, 0, finalSize.Width - tabsControl.DesiredSize.Width - addTabButton.DesiredSize.Width, tabsHeight));
        }
        else
        {
            double x = finalSize.Width - minimumWidth;
                
            tabsControl.Arrange(new Rect(0, 0, x, tabsHeight));
            addTabButton.Arrange(new Rect(x, 0, addTabButton.DesiredSize.Width, addTabButton.DesiredSize.Height));
            dragWindowThumb.Arrange(new Rect(x + addTabButton.DesiredSize.Width, 0, dragWindowThumb.DesiredSize.Width, tabsHeight));
        }
            
        return finalSize;
    }
}