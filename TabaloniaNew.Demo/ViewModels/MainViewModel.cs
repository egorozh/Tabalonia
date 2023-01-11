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
                Header = "Tab4",
                SimpleContent = "Tab 4 content"
            },
            new TabItemViewModel
            {
                Header = "Tab5",
                SimpleContent = "Tab 5 content"
            },
            new TabItemViewModel
            {
                Header = "Tab6",
                SimpleContent = "Tab 6 content"
                
            },
            new TabItemViewModel
            {
                Header = "Tab7",
                SimpleContent = "Tab 7 content"
            },
            new TabItemViewModel
            {
                Header = "Tab8",
                SimpleContent = "Tab 8 content"
            },
            new TabItemViewModel
            {
                Header = "Tab9",
                SimpleContent = "Tab 9 content"
            },
            new TabItemViewModel
            {
                Header = "Tab10",
                SimpleContent = "Tab 10 content"
            },
        };
    }
}