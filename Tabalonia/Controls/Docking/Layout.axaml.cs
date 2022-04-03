using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Tabalonia.Controls.Docking;

public class Layout : ContentControl, IStyleable
{
    #region Private Fields

    #endregion

    #region IStyleable

    Type IStyleable.StyleKey => typeof(Layout);

    #endregion

    #region Internal Properties

    #endregion

    #region Avalonia Properties
    
    #endregion

    #region Public Properties

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