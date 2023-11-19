using Avalonia.Markup.Xaml;

namespace Tabalonia.Themes.Fluent;

public class FluentTheme : Styles
{
    public FluentTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}