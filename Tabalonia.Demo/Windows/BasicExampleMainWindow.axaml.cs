using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tabalonia.Demo.ViewModels;

namespace Tabalonia.Demo.Windows
{
    public partial class BasicExampleMainWindow : Window
    {
        public BasicExampleMainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new BasicExampleMainModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
