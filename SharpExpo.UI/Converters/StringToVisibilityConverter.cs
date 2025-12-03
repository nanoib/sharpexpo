using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharpExpo.UI.Converters;

/// <summary>
/// Конвертер для преобразования строки в Visibility
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = value?.ToString();
        var isInverse = parameter?.ToString() == "Inverse";
        var isEmpty = string.IsNullOrWhiteSpace(str);
        
        if (isInverse)
        {
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

