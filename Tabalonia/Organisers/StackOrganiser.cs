using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Tabalonia.Comparers;
using Tabalonia.Controls;

namespace Tabalonia.Organisers;

public abstract class StackOrganiser : IItemsOrganiser
{
    #region Private Fields

    private readonly Orientation _orientation;
    private readonly double _itemOffset;
    private readonly Func<DragTabItem, double> _getDesiredSize;
    private readonly Func<DragTabItem, double> _getLocation;
    private readonly Action<DragTabItem, double> _setLocation;
    private readonly AvaloniaProperty _canvasProperty;

    private readonly Dictionary<DragTabItem, double> _activeStoryboardTargetLocations = new();
    private IDictionary<DragTabItem, LocationInfo>? _siblingItemLocationOnDragStart;
    private LocationInfo _dragItemLocationOnDragStart;

    #endregion

    #region Constructor

    protected StackOrganiser(Orientation orientation, double itemOffset = 0)
    {
        _orientation = orientation;
        _itemOffset = itemOffset;

        _canvasProperty = orientation == Orientation.Horizontal ? Canvas.LeftProperty : Canvas.TopProperty;

        if (orientation == Orientation.Horizontal)
        {
            _getDesiredSize = item => item.DesiredSize.Width;
            _getLocation = item => item.X;
            _setLocation = (item, coord) => { item.SetValue(DragTabItem.XProperty, coord); };
        }
        else
        {
            _getDesiredSize = item => item.DesiredSize.Height;
            _getLocation = item => item.Y;
            _setLocation = (item, coord) => { item.SetValue(DragTabItem.YProperty, coord); };
        }
    }

    #endregion

    #region Public Methods

    public void Organise(Size measureBounds, IEnumerable<DragTabItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        var sortedItems = items
            .Select((di, idx) => new Tuple<int, DragTabItem>(idx, di))
            .OrderBy(tuple => tuple,
                MultiComparer<Tuple<int, DragTabItem>>
                    .Ascending(tuple => _getLocation(tuple.Item2))
                    .ThenAscending(tuple => tuple.Item1))
            .Select(tuple => tuple.Item2);

        OrganiseInternal(measureBounds, sortedItems);
    }

    public void Organise(Size measureBounds, IOrderedEnumerable<DragTabItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        OrganiseInternal(measureBounds, items);
    }

    public void OrganiseOnDragStarted(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem)
    {
        if (siblingItems == null) throw new ArgumentNullException(nameof(siblingItems));
        if (dragItem == null) throw new ArgumentNullException(nameof(dragItem));

        _dragItemLocationOnDragStart = GetLocationInfo(dragItem);
        _siblingItemLocationOnDragStart = siblingItems.Select(GetLocationInfo).ToDictionary(loc => loc.Item);
    }

    public void OrganiseOnDrag(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem)
    {
        if (siblingItems == null) throw new ArgumentNullException(nameof(siblingItems));
        if (dragItem == null) throw new ArgumentNullException(nameof(dragItem));

        var currentLocations = GetLocations(siblingItems, dragItem);

        var currentCoord = 0.0;
        var zIndex = int.MaxValue;

        foreach (var location in currentLocations)
        {
            var item = location.Item;

            if (!Equals(item, dragItem))
            {
                SendToLocation(item, currentCoord);
                item.ZIndex = --zIndex;
            }
            
            currentCoord += _getDesiredSize(item) + _itemOffset;
        }

        dragItem.ZIndex = int.MaxValue;
    }

    public void OrganiseOnDragCompleted(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem)
    {
        if (siblingItems == null) throw new ArgumentNullException(nameof(siblingItems));

        var currentLocations = GetLocations(siblingItems, dragItem);

        var currentCoord = 0.0;
        var z = int.MaxValue;
        var logicalIndex = 0;

        foreach (var location in currentLocations)
        {
            var item = location.Item;

            _setLocation(item, currentCoord);
            currentCoord += _getDesiredSize(item) + _itemOffset;
            item.ZIndex = --z;
            item.LogicalIndex = logicalIndex++;
        }

        dragItem.ZIndex = int.MaxValue;
    }

    public Point ConstrainLocation(TabsItemsPresenter requestor, Rect measureBounds, Point itemDesiredLocation)
    {
        var fixedItems = requestor.FixedItemCount;
        var lowerBound = fixedItems == 0
            ? -1d
            : GetLocationInfo(requestor.DragablzItems()
                .Take(fixedItems)
                .Last()).End + _itemOffset - 1;

        var x = _orientation == Orientation.Vertical ? 0
            : Math.Min(Math.Max(lowerBound, itemDesiredLocation.X), (measureBounds.Width) + 1);

        var y = _orientation == Orientation.Horizontal ? 0
            : Math.Min(Math.Max(lowerBound, itemDesiredLocation.Y), (measureBounds.Height) + 1);

        return new Point(x, y);
    }

    public Size Measure(TabsItemsPresenter requestor, Rect availableSize, IEnumerable<DragTabItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

        double width = 0, height = 0;
        var isFirst = true;

        foreach (var dragTabItem in items)
        {
            dragTabItem.Measure(size);

            //var loaded = dragTabItem.IsLoaded
            var loaded = true;


            if (_orientation == Orientation.Horizontal)
            {
                height = Math.Max(height,
                    !loaded ? dragTabItem.DesiredSize.Height : dragTabItem.Bounds.Height);
                width += !loaded ? dragTabItem.DesiredSize.Width : dragTabItem.Bounds.Width;
                if (!isFirst)
                    width += _itemOffset;
            }
            else
            {
                width = Math.Max(width,
                    !loaded ? dragTabItem.DesiredSize.Width : dragTabItem.Bounds.Width);
                height += !loaded ? dragTabItem.DesiredSize.Height : dragTabItem.Bounds.Height;
                if (!isFirst)
                    height += _itemOffset;
            }

            isFirst = false;
        }

        return new Size(Math.Max(width, 0), Math.Max(height, 0));
    }

    public IEnumerable<DragTabItem> Sort(IEnumerable<DragTabItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        return items.OrderBy(i => GetLocationInfo(i).Start);
    }
    
    #endregion
    
    #region Private Methods

    private void OrganiseInternal(Size measureBounds, IEnumerable<DragTabItem> items)
    {
        var currentCoord = 0.0;
        var z = int.MaxValue;
        var logicalIndex = 0;
        foreach (var newItem in items)
        {
            newItem.ZIndex = newItem.IsSelected ? int.MaxValue : --z;
            _setLocation(newItem, currentCoord);
            newItem.LogicalIndex = logicalIndex++;
            newItem.Measure(measureBounds);
            var desiredSize = _getDesiredSize(newItem);
            if (desiredSize == 0.0) desiredSize = 1.0; //no measure? create something to help sorting
            currentCoord += desiredSize + _itemOffset;
        }
    }

    private IEnumerable<LocationInfo> GetLocations(IEnumerable<DragTabItem> siblingItems, DragTabItem dragItem)
    {
        double OrderSelector(LocationInfo loc)
        {
            if (Equals(loc.Item, dragItem))
            {
                return loc.Start > _dragItemLocationOnDragStart.Start ? loc.End : loc.Start;
            }

            return _siblingItemLocationOnDragStart[loc.Item].Mid;
        }

        var currentLocations = siblingItems
            .Select(GetLocationInfo)
            .Union(new[] { GetLocationInfo(dragItem) })
            .OrderBy(OrderSelector);

        return currentLocations;
    }

    private async void SendToLocation(DragTabItem dragTabItem, double location)
    {
        if (Math.Abs(_getLocation(dragTabItem) - location) < 1.0
            ||
            _activeStoryboardTargetLocations.TryGetValue(dragTabItem, out var activeTarget)
            && Math.Abs(activeTarget - location) < 1.0)
        {
            return;
        }

        _activeStoryboardTargetLocations[dragTabItem] = location;

        var animation = new Animation
        {
            Easing = new CubicEaseOut(),
            Duration = TimeSpan.FromMilliseconds(200),
            PlaybackDirection = PlaybackDirection.Normal,
            FillMode = FillMode.None,
            Children =
            {
                new KeyFrame
                {
                    KeyTime = TimeSpan.FromMilliseconds(200),
                    Setters =
                    {
                        new Setter(_canvasProperty, location),
                    }
                }
            }
        };

        await animation.RunAsync(dragTabItem,null);
       
        _setLocation(dragTabItem, location);
        _activeStoryboardTargetLocations.Remove(dragTabItem);
        
    }

    private LocationInfo GetLocationInfo(DragTabItem item)
    {
        var size = _getDesiredSize(item);
        if (!_activeStoryboardTargetLocations.TryGetValue(item, out var startLocation))
            startLocation = _getLocation(item);
        var midLocation = startLocation + size / 2;
        var endLocation = startLocation + size;

        return new LocationInfo(item, startLocation, midLocation, endLocation);
    }
    
    #endregion
    
    #region Private Structs

    private readonly record struct LocationInfo(DragTabItem Item, double Start, double Mid, double End);

    #endregion
}