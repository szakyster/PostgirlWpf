using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Converters;

public class BodyTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BodyType bodyType)
        {
            // BodyType.None → elrejtjük a body editort
            return bodyType == BodyType.None
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}