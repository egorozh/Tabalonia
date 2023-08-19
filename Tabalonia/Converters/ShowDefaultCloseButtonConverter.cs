using System.Globalization;


namespace Tabalonia.Converters;


public class ShowDefaultCloseButtonConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // [0] is owning TabsControl ShowDefaultCloseButton value.
        // [1] is owning TabsControl FixedHeaderCount value.
        // [2] is item LogicalIndex
        if (values.Count == 3)
        {
            if (values[0] is not bool showDefaultCloseButton ||
                values[1] is not int fixedHeaderCount ||
                values[2] is not int logicalIndex)
                return false;
            
            return showDefaultCloseButton && logicalIndex >= fixedHeaderCount;
        }

        return false;
    }
}