using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Postgirl.Converters
{
    public class StatusCodeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int statusCode && statusCode == 0)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
