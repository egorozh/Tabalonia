using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;


namespace Tabalonia.Themes;


public abstract class BaseTheme: Styles
{
    private Styles _theme = new();


    protected abstract string ResourceString { get; }
    

    protected BaseTheme(IServiceProvider serviceProvider)
    {
        var ctx = serviceProvider.GetService(typeof(IUriContext)) as IUriContext 
                  ?? throw new NullReferenceException("Unable retrieve UriContext");
       
        InitStyles(ctx.BaseUri);
    }
    

    private void InitStyles(Uri baseUri)
    {
        _theme = new Styles
        {
            new StyleInclude(baseUri)
            {
                Source = new Uri(ResourceString)
            }
        };
        
        Add(_theme);
    }
}