using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tabalonia.Controls;

namespace Tabalonia.Demo.Windows;

public class QuickStartWindow : Window
{
    public QuickStartWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var tabsControl = this.FindControl<TabsControl>("TabsControl");
        tabsControl.NewItemFactory = NewItemFactory;
    }

    private int _i;

    private object NewItemFactory() => new TabItem
    {
        Header = $"New item {_i++}"
    };
}