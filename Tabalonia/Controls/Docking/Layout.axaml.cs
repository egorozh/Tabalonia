using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Tabalonia.Controls.Docking;

public class Layout : ContentControl, IStyleable
{
    #region Private Fields

    private const string TopDropZonePartName = "PART_TopDropZone";
    private const string RightDropZonePartName = "PART_RightDropZone";
    private const string BottomDropZonePartName = "PART_BottomDropZone";
    private const string LeftDropZonePartName = "PART_LeftDropZone";

    private bool _isParticipatingInDrag;

    #endregion

    #region IStyleable

    Type IStyleable.StyleKey => typeof(Layout);

    #endregion

    #region Internal Properties

    #endregion

    #region Avalonia Properties

    public static readonly DirectProperty<Layout, bool> IsParticipatingInDragProperty =
        AvaloniaProperty.RegisterDirect<Layout, bool>(nameof(IsParticipatingInDrag),
            o => o.IsParticipatingInDrag, (o, v) => o.IsParticipatingInDrag = v);

    #endregion

    #region Public Properties

    public bool IsParticipatingInDrag
    {
        get => _isParticipatingInDrag;
        private set => SetAndRaise(IsParticipatingInDragProperty, ref _isParticipatingInDrag, value);
    }

    #endregion

    #region Routed Events

    #endregion

    #region Events

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }
}