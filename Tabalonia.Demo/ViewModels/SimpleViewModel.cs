using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Tabalonia.Demo.ViewModels;

public partial class SimpleViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;
        
    public string Name { get; set; }

    public object SimpleContent { get; set; }
}