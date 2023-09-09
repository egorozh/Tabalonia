using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;


namespace Tabalonia.Themes;


public class FluentTheme : Styles
{
    private Styles _fluentDark = new();
    
    
    public FluentTheme(IServiceProvider serviceProvider)
    {
        var ctx = serviceProvider.GetService(typeof(IUriContext)) as IUriContext 
                  ?? throw new NullReferenceException("Unable retrieve UriContext");
       
        InitStyles(ctx.BaseUri);
    }
    

    private void InitStyles(Uri baseUri)
    {
        _fluentDark = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri("avares://Tabalonia/Themes/FluentDark.axaml")
            }
        };
        
        Add(_fluentDark);
    }
}