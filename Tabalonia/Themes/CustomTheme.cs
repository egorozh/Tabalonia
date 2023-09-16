namespace Tabalonia.Themes;


public class CustomTheme : BaseTheme
{
    protected override string ResourceString => "avares://Tabalonia/Themes/Custom/Custom.axaml";


    public CustomTheme(IServiceProvider serviceProvider)
        :base(serviceProvider)
    {
    }
}