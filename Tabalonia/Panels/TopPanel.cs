namespace Tabalonia.Panels;


public class TopPanel : Panel
{
    private Layoutable LeftDragWindowThumb => Children[0];
    private Layoutable TabsControl => Children[1];
    private Layoutable AddTabButton => Children[2];
    private Layoutable RightDragWindowThumb => Children[3];
    
    
    protected override Size MeasureOverride(Size availableSize)
    {
        double height = 0;
        double width = 0;
        
        double availableWidth = availableSize.Width;
        double availableHeight = availableSize.Height;
        
        if (Children.Count != 4)
            return new Size(width, height);
        
        MeasureControl(AddTabButton, ref width, ref availableWidth, availableHeight);
        MeasureControl(LeftDragWindowThumb, ref width, ref availableWidth, availableHeight);
        MeasureControl(RightDragWindowThumb, ref width, ref availableWidth, availableHeight);
        
        TabsControl.Measure(new Size(availableWidth, availableHeight));
            
        width += TabsControl.DesiredSize.Width; 
            
        height = Math.Max(TabsControl.DesiredSize.Height, height);
            
        return new Size(width, height);
        
        static void MeasureControl(Layoutable control, ref double w, ref double aW, in double h)
        {
            control.Measure(new Size(aW, h));
            w += control.DesiredSize.Width;
            aW -= control.DesiredSize.Width; 
        }
    }
        
   
    protected override Size ArrangeOverride(Size finalSize)
    {
        double minimumWidth = LeftDragWindowThumb.DesiredSize.Width + AddTabButton.DesiredSize.Width + RightDragWindowThumb.DesiredSize.Width;

        double tabsWidth = TabsControl.DesiredSize.Width;
        double tabsHeight = TabsControl.DesiredSize.Height;

        double x;
        
        if (tabsWidth < finalSize.Width - minimumWidth)
        {
            x = LeftDragWindowThumb.DesiredSize.Width;
            
            LeftDragWindowThumb.Arrange(new Rect(0, 0, x, tabsHeight));
            
            TabsControl.Arrange(new Rect(x, 0, tabsWidth, tabsHeight));
            x += tabsWidth;
            
            ArrangeAddTabButton(AddTabButton, x, tabsHeight);
            x += AddTabButton.DesiredSize.Width;
            
            RightDragWindowThumb.Arrange(new Rect(x, 0, finalSize.Width - TabsControl.DesiredSize.Width - AddTabButton.DesiredSize.Width - LeftDragWindowThumb.DesiredSize.Width, tabsHeight));
        }
        else
        {
            x = finalSize.Width - minimumWidth;
                
            LeftDragWindowThumb.Arrange(new Rect(0, 0, LeftDragWindowThumb.DesiredSize.Width, tabsHeight));
            TabsControl.Arrange(new Rect(LeftDragWindowThumb.DesiredSize.Width, 0, x, tabsHeight));
            x += LeftDragWindowThumb.DesiredSize.Width;
            
            ArrangeAddTabButton(AddTabButton, x, tabsHeight);
            x += AddTabButton.DesiredSize.Width;
            
            RightDragWindowThumb.Arrange(new Rect(x, 0, RightDragWindowThumb.DesiredSize.Width, tabsHeight));
        }
            
        return finalSize;
    }

    
    private static void ArrangeAddTabButton(Layoutable addTabButton, double x, double parentHeight)
    {
        double verticalMargin = (parentHeight - addTabButton.DesiredSize.Height) / 2;
        
        addTabButton.Arrange(new Rect(x, verticalMargin, addTabButton.DesiredSize.Width, addTabButton.DesiredSize.Height));
    }
}