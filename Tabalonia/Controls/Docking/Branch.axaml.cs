using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;

namespace Tabalonia.Controls.Docking;

public class Branch : TemplatedControl, IStyleable
{
    #region Constants

    private const string FirstContentPresenterPartName = "PART_FirstContentPresenter";
    private const string SecondContentPresenterPartName = "PART_SecondContentPresenter";

    #endregion
    
    #region IStyleable

    Type IStyleable.StyleKey => typeof(Branch);

    #endregion

    #region Avalonia Properties

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Branch, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<object> FirstItemProperty =
        AvaloniaProperty.Register<Branch, object>(nameof(FirstItem));

    public static readonly StyledProperty<GridLength> FirstItemLengthProperty =
        AvaloniaProperty.Register<Branch, GridLength>(nameof(FirstItemLength),
            new GridLength(0.49999, GridUnitType.Star), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object> SecondItemProperty =
        AvaloniaProperty.Register<Branch, object>(nameof(SecondItem));

    public static readonly StyledProperty<GridLength> SecondItemLengthProperty =
        AvaloniaProperty.Register<Branch, GridLength>(nameof(SecondItemLength),
            new GridLength(0.50001, GridUnitType.Star), defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region Internal Properties

    internal ContentPresenter FirstContentPresenter { get; private set; }
    internal ContentPresenter SecondContentPresenter { get; private set; }

    #endregion

    #region Public Properties

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public object FirstItem
    {
        get => GetValue(FirstItemProperty);
        set => SetValue(FirstItemProperty, value);
    }

    public GridLength FirstItemLength
    {
        get => GetValue(FirstItemLengthProperty);
        set => SetValue(FirstItemLengthProperty, value);
    }

    public object SecondItem
    {
        get => GetValue(SecondItemProperty);
        set => SetValue(SecondItemProperty, value);
    }

    public GridLength SecondItemLength
    {
        get => GetValue(SecondItemLengthProperty);
        set => SetValue(SecondItemLengthProperty, value);
    }

    #endregion

    /// <summary>
    /// Gets the proportional size of the first item, between 0 and 1, where 1 would represent the entire size of the branch.
    /// </summary>
    /// <returns></returns>
    public double GetFirstProportion() =>
        (1 / (FirstItemLength.Value + SecondItemLength.Value)) * FirstItemLength.Value;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        FirstContentPresenter = e.Find<ContentPresenter>(FirstContentPresenterPartName);
        SecondContentPresenter = e.Find<ContentPresenter>(SecondContentPresenterPartName);
    }
}