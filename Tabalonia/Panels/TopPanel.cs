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
        
        if (Children.Count != 4)
            return new Size(width, height);
        
        double availableWidth = availableSize.Width;
        double availableHeight = availableSize.Height;
        
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
        if (Children.Count != 4)
            return finalSize;
        
        double withoutTabsWidth = LeftDragWindowThumb.DesiredSize.Width + AddTabButton.DesiredSize.Width + RightDragWindowThumb.DesiredSize.Width;

        double tabsWidth = TabsControl.DesiredSize.Width;
        double tabsHeight = TabsControl.DesiredSize.Height;
        double availableTabsWidth = finalSize.Width - withoutTabsWidth;
        
        if (tabsWidth < availableTabsWidth)
        {
            ArrangeWhenTabsFit(tabsWidth, tabsHeight, finalSize.Width);
            return finalSize;
        }

        ArrangeWhenTabsUnfit(tabsHeight, availableTabsWidth);
        return finalSize;
    }
    
    
    private void ArrangeWhenTabsUnfit(in double tabsHeight, in double availableTabsWidth)
    {
        double leftThumbWidth = LeftDragWindowThumb.DesiredSize.Width;
        LeftDragWindowThumb.Arrange(new Rect(0, 0, leftThumbWidth, tabsHeight));
        
        double x = leftThumbWidth;
        
        TabsControl.Arrange(new Rect(x, 0, availableTabsWidth, tabsHeight));

        x += availableTabsWidth;
        
        double addTabButtonWidth = ArrangeAddTabButton(AddTabButton, x, tabsHeight);
        x += addTabButtonWidth;
            
        RightDragWindowThumb.Arrange(new Rect(x, 0, RightDragWindowThumb.DesiredSize.Width, tabsHeight));
    }
    

    private void ArrangeWhenTabsFit(in double tabsWidth, in double tabsHeight, in double finalWidth)
    {
        double leftThumbWidth = LeftDragWindowThumb.DesiredSize.Width;
        LeftDragWindowThumb.Arrange(new Rect(0, 0, leftThumbWidth, tabsHeight));
            
        double x = leftThumbWidth;
        
        TabsControl.Arrange(new Rect(x, 0, tabsWidth, tabsHeight));
        x += tabsWidth;
            
        double addTabButtonWidth = ArrangeAddTabButton(AddTabButton, x, tabsHeight);
        x += addTabButtonWidth;

        double availableSpaceWidth = finalWidth - tabsWidth - addTabButtonWidth - leftThumbWidth;
        
        RightDragWindowThumb.Arrange(new Rect(x, 0, availableSpaceWidth, tabsHeight));
    }


    private static double ArrangeAddTabButton(Layoutable addTabButton, double x, double parentHeight)
    {
        double verticalMargin = (parentHeight - addTabButton.DesiredSize.Height) / 2;
        
        addTabButton.Arrange(new Rect(x, verticalMargin, addTabButton.DesiredSize.Width, addTabButton.DesiredSize.Height));

        return addTabButton.DesiredSize.Width;
    }
}