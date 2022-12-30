using CommunityToolkit.Mvvm.ComponentModel;

namespace TabaloniaNew.Demo.ViewModels
{
    public class TabItemViewModel : ObservableObject
    {
        public string Header { get; set; }
        
        public string SimpleContent { get; set; }
    }
}