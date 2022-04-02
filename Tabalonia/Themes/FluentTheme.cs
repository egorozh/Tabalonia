using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Tabalonia.Themes;

public class FluentTheme : IStyle, IResourceProvider
{
    private readonly Uri _baseUri;
    private IStyle[]? _loaded;
    private bool _isLoading;
    
    public FluentTheme(Uri baseUri)
    {
        _baseUri = baseUri;
    }
    
    public FluentTheme(IServiceProvider serviceProvider)
    {
        _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
    }

    /// <summary>
    /// Gets or sets the mode of the fluent theme (light, dark).
    /// </summary>
    public FluentThemeMode Mode { get; set; }

    public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

    /// <summary>
    /// Gets the loaded style.
    /// </summary>
    public IStyle Loaded
    {
        get
        {
            if (_loaded == null)
            {
                _isLoading = true;
                var loaded = (IStyle)AvaloniaXamlLoader.Load(GetUri(), _baseUri);
                _loaded = new[] { loaded };
                _isLoading = false;
            }

            return _loaded?[0]!;
        }
    }

    bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

    IReadOnlyList<IStyle> IStyle.Children => _loaded ?? Array.Empty<IStyle>();

    public event EventHandler OwnerChanged
    {
        add
        {
            if (Loaded is IResourceProvider rp)
            {
                rp.OwnerChanged += value;
            }
        }
        remove
        {
            if (Loaded is IResourceProvider rp)
            {
                rp.OwnerChanged -= value;
            }
        }
    }

    public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

    public bool TryGetResource(object key, out object? value)
    {
        if (!_isLoading && Loaded is IResourceProvider p)
        {
            return p.TryGetResource(key, out value);
        }

        value = null;
        return false;
    }

    void IResourceProvider.AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);
    void IResourceProvider.RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

    private Uri GetUri() => Mode switch
    {
        FluentThemeMode.Dark => new Uri("avares://Tabalonia/Themes/FluentDark.axaml", UriKind.Absolute),
        _ => new Uri("avares://Tabalonia/Themes/FluentLight.axaml", UriKind.Absolute),
    };
}