using System.Globalization;
using Avalonia.Data.Converters;
using Tabalonia.Docking;

namespace Tabalonia.Converters.Docking;

public class DropZoneShapeSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DropZoneLocation location && parameter is string toWidth)
            return GetSize(location, toWidth);

        return 160;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private double GetSize(DropZoneLocation location, string toWidth)
    {
        double width, height;

        switch (location)
        {
            case DropZoneLocation.Top:
            case DropZoneLocation.Bottom:
                width = 160;
                height = 80;
                break;

            default:
                width = 80;
                height = 160;
                break;
        }

        return toWidth == "width" ? width : height;
    }
}