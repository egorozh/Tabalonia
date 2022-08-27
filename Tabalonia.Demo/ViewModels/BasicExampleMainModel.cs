using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tabalonia.Demo.ViewModels;

public partial class BasicExampleMainModel : ObservableObject
{
    private int _newViewModelCount;

    [ObservableProperty] private SimpleViewModel _selectedViewModel;
    [ObservableProperty] private string _peopleMonitorText = "awaiting...";
    [ObservableProperty] private string _basicColourMonitorText = "awaiting...";
    [ObservableProperty] private Func<object> _newItemFactory;
    
    //private readonly VerticalPositionMonitor _peopleMonitor;


    //public PositionMonitor BasicColourMonitor { get; }

    //public PositionMonitor PeopleMonitor
    //{
    //    get { return _peopleMonitor; }
    //}
    
    public ObservableCollection<SimpleViewModel> ViewModels { get; } = new();


    //public IInterTabClient BasicInterTabClient { get; }
    public BasicExampleMainModel()
    {
        //BasicColourMonitor = new PositionMonitor();
        //BasicColourMonitor.LocationChanged += (sender, args) => BasicColourMonitorText = args.Location.ToString();
        //_peopleMonitor = new VerticalPositionMonitor();
        //_peopleMonitor.OrderChanged += PeopleMonitorOnOrderChanged;
        
        ViewModels.Add(new SimpleViewModel {Name = "Alpha", SimpleContent = "This is the alpha content"});
        ViewModels.Add(new SimpleViewModel {Name = "Beta", SimpleContent = "Beta content", IsSelected = true});
        ViewModels.Add(new SimpleViewModel {Name = "Gamma", SimpleContent = "And here is the gamma content"});

        SelectedViewModel = ViewModels[1];

        //BasicInterTabClient = new BasicExampleInterTabClient();
        _newItemFactory = AddNewViewModel;
    }
    
    private object AddNewViewModel()
    {
        _newViewModelCount++;

        return new SimpleViewModel
        {
            Name = "New Tab " + _newViewModelCount,
            SimpleContent = "New Tab Content " + _newViewModelCount
        };
    }


    //private void PeopleMonitorOnOrderChanged(object sender, OrderChangedEventArgs orderChangedEventArgs)
    //{
    //    PeopleMonitorText = orderChangedEventArgs.NewOrder.OfType<Person>()
    //        .Aggregate("", (accumalate, person) => accumalate + person.LastName + ", ");
    //}
}