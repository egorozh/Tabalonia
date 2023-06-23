using System.Globalization;


namespace TabaloniaNew.Converters;


public class ShowDefaultCloseButtonConverter : IMultiValueConverter
{
    /// <summary>
    /// [0] is owning tabcontrol ShowDefaultCloseButton value.
    /// [1] is item LogicalIndex
    /// </summary>
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count == 2)
        {
            bool showDefaultCloseButton = values[0] != AvaloniaProperty.UnsetValue && (bool) values[0];
            int logicalIndex = values[1] == AvaloniaProperty.UnsetValue ? 0 : (int) values[1];

            return showDefaultCloseButton;
        }

        return false;
    }
}