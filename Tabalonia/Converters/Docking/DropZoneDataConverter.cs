using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Tabalonia.Docking;

namespace Tabalonia.Converters.Docking;

public class DropZoneDataConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DropZoneLocation location)
            return CreateData(location);

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #region Private Methods

    private PathGeometry CreateData(DropZoneLocation location)
    {
        Point startPoint, endPoint;

        switch (location)
        {
            case DropZoneLocation.Top:
                startPoint = new Point(160, 0);
                endPoint = new Point(0, 0);
                break;
            case DropZoneLocation.Right:
                startPoint = new Point(60, 160);
                endPoint = new Point(60, 0);
                break;
            case DropZoneLocation.Bottom:
                startPoint = new Point(0, 60);
                endPoint = new Point(160, 60);
                break;
            case DropZoneLocation.Left:
                endPoint = new Point(0, 160);
                startPoint = new Point(0, 0);
                break;
            default:
                startPoint = new Point();
                endPoint = new Point();
                break;
        }


        ArcSegment arcSegment = new ArcSegment()
        {
            SweepDirection = SweepDirection.Clockwise,
            Size = new Size(60, 60),
            Point = endPoint
        };

        var figure = new PathFigure
        {
            IsClosed = true,
            IsFilled = true,
            StartPoint = startPoint,
            Segments = new PathSegments {arcSegment}
        };


        return new PathGeometry {Figures = new PathFigures {figure}};
    }

    #endregion
}