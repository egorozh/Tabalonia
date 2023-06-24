using Avalonia.Data.Converters;
using System.Globalization;
using Avalonia;

namespace Tabalonia.Converters;

public class ShowDefaultCloseButtonConverter : IMultiValueConverter
{
    /// <summary>
    /// [0] is owning tabcontrol ShowDefaultCloseButton value.
    /// [1] is owning tabcontrol FixedHeaderCount value.
    /// [2] is item LogicalIndex
    /// </summary>
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count == 3)
        {
            bool showDefaultCloseButton = values[0] != AvaloniaProperty.UnsetValue && (bool) values[0];
            int fixedHeaderCount = values[1] == AvaloniaProperty.UnsetValue ? 0 : (int) values[1];
            int logicalIndex = values[2] == AvaloniaProperty.UnsetValue ? 0 : (int) values[2];

            return showDefaultCloseButton && logicalIndex >= fixedHeaderCount;
        }

        return false;
    }
}