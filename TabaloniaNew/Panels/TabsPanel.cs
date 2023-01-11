using static System.Math;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TabaloniaNew.Controls;


namespace TabaloniaNew.Panels
{
    public class TabsPanel : Panel
    {
        private readonly Dictionary<DragTabItem, LocationInfo> _itemsLocations = new();
        private double _itemWidth;
        private readonly Dictionary<DragTabItem, double> _activeStoryboardTargetLocations = new();

        public TabsPanel()
        {
            Background = new SolidColorBrush(Color.Parse("#33ff0000"));
        }


        public double ItemWidth { get; internal set; }

        public double ItemOffset { get; internal set; }


        protected override Size MeasureOverride(Size availableSize)
        {
            var draggedItem = (DragTabItem?) Children.FirstOrDefault(c => c is DragTabItem
            {
                IsDragging: true
            });

            return draggedItem is not null 
                ? DragMeasureImpl(draggedItem, availableSize) 
                : MeasureImpl(availableSize);
        }
        

        protected override Size ArrangeOverride(Size finalSize)
        {
            var draggedItem = (DragTabItem?) Children.FirstOrDefault(c => c is DragTabItem
            {
                IsDragging: true
            });
            
            return draggedItem is not null 
                ? DragArrangeImpl(draggedItem, finalSize) 
                : ArrangeImpl(finalSize);
        }
        

        private Size MeasureImpl(Size availableSize)
        {
            _itemWidth = GetAvailableWidth(availableSize);

            double height = 0;
            double width = 0;

            bool isFirst = true;

            foreach (var tabItem in Children)
            {
                tabItem.Measure(new Size(_itemWidth, availableSize.Height));

                width += _itemWidth;
                height = Max(tabItem.DesiredSize.Height, height);

                if (!isFirst)
                    width += ItemOffset;

                isFirst = false;
            }

            return new Size(width, height);
        }


        private Size DragMeasureImpl(DragTabItem draggedItem, Size availableSize)
        {
            double height = 0;
            double width = 0;

            bool isFirst = true;

            foreach (var tabItem in Children)
            {
                tabItem.Measure(new Size(_itemWidth, availableSize.Height));
                
                width += _itemWidth;
                height = Max(tabItem.DesiredSize.Height, height);

                if (!isFirst)
                    width += ItemOffset;

                isFirst = false;
            }
            
            if (draggedItem.X + _itemWidth > width)
                return new Size(draggedItem.X + _itemWidth, height);
            
            return new Size(width, height);
        }
        
        
        private Size ArrangeImpl(Size finalSize)
        {
            double x = 0;
            int z = int.MaxValue;

            _itemsLocations.Clear();
            
            foreach (IControl? child in Children)
            {
                if (child is not DragTabItem tabItem)
                    continue;

                tabItem.ZIndex = tabItem.IsSelected ? int.MaxValue : --z;
                
                SetLocation(tabItem, x, _itemWidth);
                
                _itemsLocations.Add(tabItem, GetLocationInfo(tabItem));

                x += _itemWidth + ItemOffset;
            }

            return finalSize;
        }

        
        private Size DragArrangeImpl(DragTabItem dragItem, Size finalSize)
        {
            var dragItemsLocations = GetLocations(Children.OfType<DragTabItem>(), dragItem);
            
            int zIndex = int.MaxValue;
            double currentCoord = 0.0;
            
            
            foreach (var location in dragItemsLocations)
            {
                var item = location.Item;

                if (!Equals(item, dragItem))
                {
                    SendToLocation(item, currentCoord, _itemWidth);
                    item.ZIndex = --zIndex;
                }
                else
                {
                    SetLocation(dragItem, dragItem.X, _itemWidth);
                }
                
                currentCoord += _itemWidth + ItemOffset;
            }
            
            dragItem.ZIndex = int.MaxValue;
            
            return finalSize;
        }


        private double GetAvailableWidth(Size availableSize)
        {
            int tabsCount = Children.Count;

            if (tabsCount == 0)
                return 0;

            double itemWidth = availableSize.Width / tabsCount - ItemOffset * (tabsCount - 1) / tabsCount;

            return Min(ItemWidth, itemWidth);
        }

        
        private IEnumerable<LocationInfo> GetLocations(IEnumerable<DragTabItem> allItems, DragTabItem dragItem)
        {
            double OrderSelector(LocationInfo loc)
            {
                if (Equals(loc.Item, dragItem))
                {
                    var dragItemInfo = _itemsLocations[dragItem];
                    
                    return loc.Start > dragItemInfo.Start ? loc.End : loc.Start;
                }

                return _itemsLocations[loc.Item].Mid;
            }

            var currentLocations = allItems
                .Select(GetLocationInfo)
                .OrderBy(OrderSelector);

            return currentLocations;
        }
        
        
        private void SendToLocation(DragTabItem item, double location, double width)
        {
            if (Abs(item.X - location) < 1.0
                || _activeStoryboardTargetLocations.TryGetValue(item, out double activeTarget) && Abs(activeTarget - location) < 1.0)
            {
                return;
            }

            _activeStoryboardTargetLocations[item] = location;
            
            // var animation = new Animation
            // {
            //     Easing = new CubicEaseOut(),
            //     Duration = TimeSpan.FromMilliseconds(200),
            //     PlaybackDirection = PlaybackDirection.Normal,
            //     FillMode = FillMode.None,
            //     Children =
            //     {
            //         new KeyFrame
            //         {
            //             KeyTime = TimeSpan.FromMilliseconds(200),
            //             Setters =
            //             {
            //                 new Setter(_canvasProperty, location),
            //             }
            //         }
            //     }
            // };

            // DoubleTransition an = new()
            // {
            //     Easing = new CubicEaseOut(),
            //     Duration = TimeSpan.FromMilliseconds(200)
            // };
            //
            // var subject = new Subject<double>();
            //
            // subject.Subscribe(
            //     onNext: x => { SetLocation(item, x, width); },
            //     onCompleted: () =>
            //     {
            //       
            //     });
            //
            // an.DoTransition(subject, item.X, location);
            // an.Apply(item, null, item.X, location);

            //await animation.RunAsync(item, null);

            SetLocation(item, location, width);
            
            _activeStoryboardTargetLocations.Remove(item);
        }


        private static void SetLocation(DragTabItem dragTabItem, double x, double width)
        {
            const double y = 0;

            dragTabItem.X = x;
            dragTabItem.Y = y;

            dragTabItem.Arrange(new Rect(new Point(x, y), new Size(width, dragTabItem.DesiredSize.Height)));
        }
        
        
        private LocationInfo GetLocationInfo(DragTabItem item)
        {
            double size = item.Bounds.Width;
            
            if (!_activeStoryboardTargetLocations.TryGetValue(item, out double startLocation))
                startLocation = item.X;
            
            double midLocation = startLocation + size / 2;
            double endLocation = startLocation + size;

            return new LocationInfo(item, startLocation, midLocation, endLocation);
        }
        
        
        #region Private Structs

        private readonly record struct LocationInfo(DragTabItem Item, double Start, double Mid, double End);

        #endregion
    }
}