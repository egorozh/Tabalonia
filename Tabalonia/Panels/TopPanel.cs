namespace Tabalonia.Panels;


public class TopPanel : StackPanel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double height = 0;
        double width = 0;

        double availableWidth = availableSize.Width;
        double availableHeight = availableSize.Height;
            
        if (Children.Count != 4)
            return new Size(width, height);
            
        var leftDragWindowThumb = Children[0];
        var tabsControl = Children[1];
        var addTabButton = Children[2];
        var rightDragWindowThumb = Children[3];
            
        addTabButton.Measure(new Size(availableWidth, availableHeight));
        width += addTabButton.DesiredSize.Width;
        availableWidth -= addTabButton.DesiredSize.Width;
        
        leftDragWindowThumb.Measure(new Size(availableWidth, availableHeight));
        width += leftDragWindowThumb.DesiredSize.Width;
        availableWidth -= leftDragWindowThumb.DesiredSize.Width;
            
        rightDragWindowThumb.Measure(new Size(availableWidth, availableHeight));
        width += rightDragWindowThumb.DesiredSize.Width;
        availableWidth -= rightDragWindowThumb.DesiredSize.Width;
            
        tabsControl.Measure(new Size(availableWidth, availableHeight));
            
        width += tabsControl.DesiredSize.Width; 
            
        height = Math.Max(tabsControl.DesiredSize.Height, height);
            
        return new Size(width, height);
    }
        
        
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count != 4)
            return finalSize;

        var leftDragWindowThumb = Children[0];
        var tabsControl = Children[1];
        var addTabButton = Children[2];
        var rightDragWindowThumb = Children[3];
            
        double minimumWidth = leftDragWindowThumb.DesiredSize.Width + addTabButton.DesiredSize.Width + rightDragWindowThumb.DesiredSize.Width;

        double tabsWidth = tabsControl.DesiredSize.Width;
        double tabsHeight = tabsControl.DesiredSize.Height;

        double x;
        
        if (tabsWidth < finalSize.Width - minimumWidth)
        {
            x = leftDragWindowThumb.DesiredSize.Width;
            
            leftDragWindowThumb.Arrange(new Rect(0, 0, x, tabsHeight));
            
            tabsControl.Arrange(new Rect(x, 0, tabsWidth, tabsHeight));
            x += tabsWidth;
            
            ArrangeAddTabButton(addTabButton, x, tabsHeight);
            x += addTabButton.DesiredSize.Width;
            
            rightDragWindowThumb.Arrange(new Rect(x, 0, finalSize.Width - tabsControl.DesiredSize.Width - addTabButton.DesiredSize.Width - leftDragWindowThumb.DesiredSize.Width, tabsHeight));
        }
        else
        {
            x = finalSize.Width - minimumWidth;
                
            leftDragWindowThumb.Arrange(new Rect(0, 0, leftDragWindowThumb.DesiredSize.Width, tabsHeight));
            tabsControl.Arrange(new Rect(leftDragWindowThumb.DesiredSize.Width, 0, x, tabsHeight));
            x += leftDragWindowThumb.DesiredSize.Width;
            
            ArrangeAddTabButton(addTabButton, x, tabsHeight);
            x += addTabButton.DesiredSize.Width;
            
            rightDragWindowThumb.Arrange(new Rect(x, 0, rightDragWindowThumb.DesiredSize.Width, tabsHeight));
        }
            
        return finalSize;
    }

    
    private static void ArrangeAddTabButton(Control addTabButton, double x, double parentHeight)
    {
        double verticalMargin = (parentHeight - addTabButton.DesiredSize.Height) / 2;
        
        addTabButton.Arrange(new Rect(x, verticalMargin, addTabButton.DesiredSize.Width, addTabButton.DesiredSize.Height));
    }
}