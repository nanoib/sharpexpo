using System.Globalization;
using System.Windows.Data;

namespace SharpExpo.UI.Converters;

/// <summary>
/// Конвертер для преобразования состояния развернутости в символ треугольника
/// </summary>
public class ExpandedToTriangleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "▼" : "▶";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

