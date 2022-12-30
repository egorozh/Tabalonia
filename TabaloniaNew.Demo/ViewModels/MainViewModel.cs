using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;


namespace TabaloniaNew.Demo.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<TabItemViewModel> TabItems { get; set; } = new()
        {
            new TabItemViewModel
            {
                Header = "Tab1",
                SimpleContent = "Tab 1 content"
            },
            new TabItemViewModel
            {
                Header = "Tab2",
                SimpleContent = "Tab 2 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
                
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            },
            new TabItemViewModel
            {
                Header = "Tab3",
                SimpleContent = "Tab 3 content"
            }
        };
    }
}