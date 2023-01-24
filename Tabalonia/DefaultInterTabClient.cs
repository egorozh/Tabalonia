using System.Collections;

namespace Tabalonia;

public class DefaultInterTabClient : IInterTabClient
{
    public virtual INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabsControl source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var sourceWindow = source.GetWindow();

        if (sourceWindow == null) throw new ApplicationException("Unable to ascertain source window.");
        var newWindow = (Window) Activator.CreateInstance(sourceWindow.GetType());

        //Dispatcher.UIThread.Post(()=>{}, DispatcherPriority.DataBind);
        
        var newTabablzControl = newWindow.LogicalTreeDepthFirstTraversal().OfType<TabsControl>().FirstOrDefault();

        if (newTabablzControl == null) throw new ApplicationException("Unable to ascertain tab control.");

        if (newTabablzControl.Items is IList items)
            items.Clear();

        return new NewTabHost<Window>(newWindow, newTabablzControl);    
    }

    public virtual TabEmptiedResponse TabEmptiedHandler(TabsControl tabControl, Window window)
    {
        return TabEmptiedResponse.CloseWindowOrLayoutBranch;
    }
}