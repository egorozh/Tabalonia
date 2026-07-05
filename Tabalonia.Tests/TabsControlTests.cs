using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Tabalonia.Controls;
using Xunit;

namespace Tabalonia.Tests;

public class TabsControlTests
{
    private static (Window Window, TabsControl Tabs, ObservableCollection<string> Items) CreateShownTabsControl(
        params string[] items)
    {
        var itemsSource = new ObservableCollection<string>(items);

        var tabs = new TabsControl
        {
            ItemsSource = itemsSource
        };

        var window = new Window { Content = tabs };
        window.Show();

        return (window, tabs, itemsSource);
    }

    [AvaloniaFact]
    public void Creates_DragTabItem_Containers_For_Items()
    {
        var (_, tabs, _) = CreateShownTabsControl("A", "B", "C");

        Assert.Equal(3, tabs.ItemCount);

        for (var i = 0; i < tabs.ItemCount; i++)
            Assert.IsType<DragTabItem>(tabs.ContainerFromIndex(i));
    }

    [AvaloniaFact]
    public void AddItemCommand_Uses_NewItemFactory_And_Selects_New_Item()
    {
        var (_, tabs, items) = CreateShownTabsControl("A");
        tabs.NewItemFactory = () => "New";

        tabs.AddItemCommand.Execute(null);

        Assert.Equal(["A", "New"], items);
        Assert.Equal("New", tabs.SelectedItem);
    }

    [AvaloniaFact]
    public void CloseItemCommand_Removes_Item_And_Raises_TabClosed()
    {
        var (_, tabs, items) = CreateShownTabsControl("A", "B");

        object? closedItem = null;
        tabs.TabClosed += (_, e) => closedItem = e.Item;

        var container = tabs.ContainerFromIndex(1);
        tabs.CloseItemCommand.Execute(container);

        Assert.Equal(["A"], items);
        Assert.Equal("B", closedItem);
    }

    [AvaloniaFact]
    public void TabClosing_Cancel_Prevents_Removal()
    {
        var (_, tabs, items) = CreateShownTabsControl("A", "B");

        tabs.TabClosing += (_, e) => e.Cancel = true;

        var container = tabs.ContainerFromIndex(1);
        tabs.CloseItemCommand.Execute(container);

        Assert.Equal(["A", "B"], items);
    }

    [AvaloniaFact]
    public void FixedHeaderCount_Defaults_To_Zero()
    {
        var (_, tabs, _) = CreateShownTabsControl("A");

        Assert.Equal(0, tabs.FixedHeaderCount);
    }
}
