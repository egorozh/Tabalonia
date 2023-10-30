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
        
        height = Math.Max(TabsControl.DesiredSize.Height, AddTabButton.DesiredSize.Height);
            
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
        
        double leftThumbWidth = LeftDragWindowThumb.DesiredSize.Width;
        double tabsWidth = TabsControl.DesiredSize.Width;
        double addTabButtonWidth = AddTabButton.DesiredSize.Width;
        double rightThumbWidth = RightDragWindowThumb.DesiredSize.Width;
        
        double tabsHeight = Math.Max(TabsControl.DesiredSize.Height, finalSize.Height);
        
        double withoutTabsWidth = leftThumbWidth + addTabButtonWidth + rightThumbWidth;
        double availableTabsWidth = finalSize.Width - withoutTabsWidth;
        
        LeftDragWindowThumb.Arrange(new Rect(0, 0, leftThumbWidth, tabsHeight));
        
        //|                         finalSize.Width                            |
        //
        //   if (tabsWidth < availableTabsWidth):
        //|leftThumb|tab1    |tab2    |addTabButton|         rightThumb        |
        //
        //   else
        //|leftThumb|tab1|tab2|tab3|tab4|tab5|tab6|tab7|addTabButton|rightThumb|
        
        if (tabsWidth < availableTabsWidth)
        {
            ArrangeWhenTabsFit(x: leftThumbWidth, leftThumbWidth, tabsWidth, addTabButtonWidth, tabsHeight, finalSize.Width);
            return finalSize;
        }
    
        ArrangeWhenTabsUnfit(x: leftThumbWidth, tabsHeight, addTabButtonWidth, rightThumbWidth, availableTabsWidth);
        return finalSize;
    }
    
    
    /// <summary>
    /// |leftThumb|tab1    |tab2    |addTabButton|         rightThumb        |
    /// </summary>
    private void ArrangeWhenTabsFit(double x, 
        in double leftThumbWidth, 
        in double tabsWidth, 
        in double addTabButtonWidth, 
        in double tabsHeight,
        in double finalWidth)
    {
        TabsControl.Arrange(new Rect(x, 0, tabsWidth, tabsHeight));
        x += tabsWidth;
            
        ArrangeCenterVertical(AddTabButton, x, tabsHeight);
        x += addTabButtonWidth;

        double availableSpaceWidth = finalWidth - tabsWidth - addTabButtonWidth - leftThumbWidth;
        
        RightDragWindowThumb.Arrange(new Rect(x, 0, availableSpaceWidth, tabsHeight));
    }

    
    /// <summary>
    /// |leftThumb|tab1|tab2|tab3|tab4|tab5|tab6|tab7|addTabButton|rightThumb|
    /// </summary>
    private void ArrangeWhenTabsUnfit(double x,
        in double tabsHeight, 
        in double addTabButtonWidth, 
        in double rightThumbWidth, 
        in double availableTabsWidth)
    {
        TabsControl.Arrange(new Rect(x, 0, availableTabsWidth, tabsHeight));

        x += availableTabsWidth;
        
        ArrangeCenterVertical(AddTabButton, x, tabsHeight);
        x += addTabButtonWidth;
            
        RightDragWindowThumb.Arrange(new Rect(x, 0, rightThumbWidth, tabsHeight));
    }
    
    
    private static void ArrangeCenterVertical(Layoutable control, double x, double fullHeight)
    {
        double width = control.DesiredSize.Width;
        double height = control.DesiredSize.Height;
        
        double y = (fullHeight - height) / 2;
        
        control.Arrange(new Rect(x, y, width, height));
    }
}