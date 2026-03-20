using System;
using System.Globalization;
using System.Windows.Data;

namespace Postgirl.Converters;

public class StringTruncateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text) return string.Empty;
        if (!int.TryParse(parameter?.ToString(), out var maxLength)) return text;

        return text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength), "…");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
