using System.Globalization;
using System.Windows.Data;

namespace SharpExpo.UI.Converters;

/// <summary>
/// Конвертер для преобразования bool в символ треугольника
/// </summary>
public class BoolToTriangleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return "▼"; // Треугольник вниз для секций
        }
        return ""; // Пусто для обычных свойств
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

