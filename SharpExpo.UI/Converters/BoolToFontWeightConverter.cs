using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharpExpo.UI.Converters;

/// <summary>
/// Конвертер для преобразования bool в FontWeight
/// </summary>
public class BoolToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return FontWeights.Bold;
        }
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

