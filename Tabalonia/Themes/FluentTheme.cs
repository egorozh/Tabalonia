using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;


namespace Tabalonia.Themes;


public class FluentTheme : Styles
{
    private Styles _fluentDark = new();
    
    
    public FluentTheme(IServiceProvider serviceProvider)
    {
        var ctx = serviceProvider.GetService(typeof(IUriContext)) as IUriContext ?? throw new NullReferenceException("Unable retrieve UriContext");
       
        InitStyles(ctx.BaseUri);
    }
    

    private void InitStyles(Uri baseUri)
    {
        string uri = OperatingSystem.IsWindows()
            ? "avares://Tabalonia/Themes/FluentDark.Win.axaml"
            : "avares://Tabalonia/Themes/FluentDark.axaml";
        
        _fluentDark = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri(uri)
            }
        };
        
        Add(_fluentDark);
    }
}