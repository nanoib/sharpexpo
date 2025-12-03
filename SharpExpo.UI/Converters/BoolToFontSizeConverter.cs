using System.Globalization;
using System.Windows.Data;

namespace SharpExpo.UI.Converters;

/// <summary>
/// Конвертер для преобразования bool в размер шрифта
/// </summary>
public class BoolToFontSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            // Для заголовков категорий - меньший размер шрифта
            return 11.0;
        }
        // Для обычного текста - стандартный размер
        return 12.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


