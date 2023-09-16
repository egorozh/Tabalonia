namespace Tabalonia.Themes;


public class FluentTheme : BaseTheme
{
    protected override string ResourceString => "avares://Tabalonia/Themes/Fluent/Fluent.axaml";


    public FluentTheme(IServiceProvider serviceProvider)
        :base(serviceProvider)
    {
    }
}