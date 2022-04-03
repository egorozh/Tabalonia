using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Tabalonia.Docking;

namespace Tabalonia.Controls.Docking;

public class DropZone : TemplatedControl, IStyleable
{
    #region Private Fields

    private bool _isOffered;

    #endregion

    #region IStyleable

    Type IStyleable.StyleKey => typeof(DropZone);

    #endregion

    #region Avalonia Properties

    public static readonly StyledProperty<DropZoneLocation> LocationProperty =
        AvaloniaProperty.Register<DropZone, DropZoneLocation>(nameof(Location));
    
    public static readonly DirectProperty<DropZone, bool> IsOfferedProperty =
        AvaloniaProperty.RegisterDirect<DropZone, bool>(nameof(IsOffered),
            o => o.IsOffered, (o, v) => o.IsOffered = v);

    #endregion

    #region Public Properties

    public DropZoneLocation Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }


    public bool IsOffered
    {
        get => _isOffered;
        internal set => SetAndRaise(IsOfferedProperty, ref _isOffered, value);
    }

    #endregion
    
   
}