namespace Tabalonia;

public class InterTabController : AvaloniaObject
{
    public InterTabController()
    {
        HorizontalPopoutGrace = 8;
        VerticalPopoutGrace = 8;
        MoveWindowWithSolitaryTabs = true;
    }

    #region Avalonia Properties

    public static readonly StyledProperty<double> HorizontalPopoutGraceProperty =
        AvaloniaProperty.Register<InterTabController, double>(nameof(HorizontalPopoutGrace), 8.0);

    public static readonly StyledProperty<double> VerticalPopoutGraceProperty =
        AvaloniaProperty.Register<InterTabController, double>(nameof(VerticalPopoutGrace), 8.0);

    public static readonly StyledProperty<bool> MoveWindowWithSolitaryTabsProperty =
        AvaloniaProperty.Register<InterTabController, bool>(nameof(MoveWindowWithSolitaryTabs), true);

    public static readonly StyledProperty<IInterTabClient> InterTabClientProperty =
        AvaloniaProperty.Register<InterTabController, IInterTabClient>(nameof(InterTabClient),
            new DefaultInterTabClient());

    #endregion

    #region Public Properties

    public double HorizontalPopoutGrace
    {
        get => GetValue(HorizontalPopoutGraceProperty);
        set => SetValue(HorizontalPopoutGraceProperty, value);
    }

    public double VerticalPopoutGrace
    {
        get => GetValue(VerticalPopoutGraceProperty);
        set => SetValue(VerticalPopoutGraceProperty, value);
    }

    public bool MoveWindowWithSolitaryTabs
    {
        get => GetValue(MoveWindowWithSolitaryTabsProperty);
        set => SetValue(MoveWindowWithSolitaryTabsProperty, value);
    }

    public IInterTabClient InterTabClient
    {
        get => GetValue(InterTabClientProperty);
        set => SetValue(InterTabClientProperty, value);
    }

    #endregion

    /// <summary>
    /// The partition allows on or more tab environments in a single application.  Only tabs which have a tab controller
    /// with a common partition will be allowed to have tabs dragged between them.  <c>null</c> is a valid partition (i.e global).
    /// </summary>
    public string Partition { get; set; }
}