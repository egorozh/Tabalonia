using Avalonia.Data.Converters;
using Avalonia.Layout;
using System.Globalization;

namespace Tabalonia.Converters;

/// <summary>
/// Provides a little help for sizing the header panel in the tab control
/// </summary>
public class TabsItemsPresenterSizeConverter : IMultiValueConverter
{        
    public Orientation Orientation { get; set; }


    /// <summary>
    /// The first value should be the total size available size, typically the parent control size.  
    /// The second value should be from <see cref="TabsItemsPresenter.ItemsPresenterWidthProperty"/> or (height equivalent)
    /// All additional values should be siblings sizes (width or height) which will affect (reduce) the available size.
    /// </summary>
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));

        if (values.Count < 2) return AvaloniaProperty.UnsetValue;

        //var val = values
        //    .Skip(2)
        //    .OfType<double>()
        //    .Where(d => !double.IsInfinity(d) && !double.IsNaN(d))
        //    .Aggregate(values.OfType<double>().First(), (current, diminish) => current - diminish);

        //var maxWidth = values.Take(2).OfType<double>().Max();
        
        if (values[0] is double tabsControlWidth)
        {
            if (values[1] is double tabItemsWidth)
                return tabItemsWidth;

            //if (values[2] is double addButtonWidth)
            //    return tabsControlWidth - addButtonWidth;
            
            return tabsControlWidth;
        }
        
        return AvaloniaProperty.UnsetValue;
    }
}