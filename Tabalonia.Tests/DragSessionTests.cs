using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Tabalonia.Controls;
using Xunit;

namespace Tabalonia.Tests;

/// <summary>
/// Cross-window drag session tests. The headless platform maps every window to the same
/// screen space (Window.Position is ignored by PointToScreen), so windows are separated
/// vertically via the tab control's Margin.
/// </summary>
public class DragSessionTests
{
    private static (Window Window, TabsControl Tabs, ObservableCollection<string> Items) CreateTabsWindow(
        double topOffset, params string[] items)
    {
        var itemsSource = new ObservableCollection<string>(items);

        var tabs = new TabsControl
        {
            ItemsSource = itemsSource,
            Margin = new Thickness(0, topOffset, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
        };

        var window = new Window { Width = 600, Height = 500, Content = tabs };
        window.Show();

        return (window, tabs, itemsSource);
    }

    private static Point TabCenter(Window window, TabsControl tabs, int index)
    {
        var container = (DragTabItem)tabs.ContainerFromIndex(index)!;
        Point topLeft = container.TranslatePoint(new Point(0, 0), window)!.Value;

        return topLeft + new Vector(container.Bounds.Width / 2, container.Bounds.Height / 2);
    }

    [AvaloniaFact]
    public void Dragging_Tab_Over_Another_Strip_Attaches_Before_Release()
    {
        var (win1, tabs1, items1) = CreateTabsWindow(0, "A", "B");
        var (win2, tabs2, items2) = CreateTabsWindow(250, "C");
        Dispatcher.UIThread.RunJobs();

        Point start = TabCenter(win1, tabs1, 1);
        win1.MouseDown(start, MouseButton.Left);
        win1.MouseMove(start + new Vector(10, 0), RawInputModifiers.LeftMouseButton);

        // Point inside win2's strip, to the right of tab "C"
        Point overSecondStrip = TabCenter(win2, tabs2, 0) + new Vector(160, 0);
        win1.MouseMove(overSecondStrip, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        // The tab must transfer while the button is still held
        Assert.Equal(["A"], items1);
        Assert.Equal(["C", "B"], items2);

        win1.MouseUp(overSecondStrip, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["C", "B"], items2);
        Assert.Equal("B", tabs2.SelectedItem);
    }

    [AvaloniaFact]
    public void Dragging_Tab_Away_Detaches_Then_Attaches_On_Hover()
    {
        var (win1, tabs1, items1) = CreateTabsWindow(0, "A", "B");
        var (win2, tabs2, items2) = CreateTabsWindow(250, "C");
        Dispatcher.UIThread.RunJobs();

        Point start = TabCenter(win1, tabs1, 1);
        win1.MouseDown(start, MouseButton.Left);
        win1.MouseMove(start + new Vector(10, 0), RawInputModifiers.LeftMouseButton);

        // Far from both strips: the tab detaches into a floating window
        var emptySpace = new Point(300, 450);
        win1.MouseMove(emptySpace, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["A"], items1);
        Assert.DoesNotContain("B", items2);

        // Hovering the second strip attaches the floating tab without releasing the button
        Point overSecondStrip = TabCenter(win2, tabs2, 0) + new Vector(160, 0);
        win1.MouseMove(overSecondStrip, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["C", "B"], items2);

        win1.MouseUp(overSecondStrip, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["C", "B"], items2);
        Assert.Equal("B", tabs2.SelectedItem);
    }

    [AvaloniaFact]
    public void Attached_Tab_Redetaches_When_Dragged_Away_From_Strip()
    {
        var (win1, tabs1, items1) = CreateTabsWindow(0, "A", "B");
        var (win2, tabs2, items2) = CreateTabsWindow(250, "C");
        Dispatcher.UIThread.RunJobs();

        Point start = TabCenter(win1, tabs1, 1);
        win1.MouseDown(start, MouseButton.Left);
        win1.MouseMove(start + new Vector(10, 0), RawInputModifiers.LeftMouseButton);

        // Attach to the second strip
        Point overSecondStrip = TabCenter(win2, tabs2, 0) + new Vector(160, 0);
        win1.MouseMove(overSecondStrip, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(["C", "B"], items2);

        // Drag away vertically: the tab must leave the strip again into a floating window
        var emptySpace = new Point(300, 450);
        win1.MouseMove(emptySpace, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["A"], items1);
        Assert.Equal(["C"], items2);

        win1.MouseUp(emptySpace, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        // The tab lives in its own floating window now
        Assert.Equal(["A"], items1);
        Assert.Equal(["C"], items2);
    }
}
