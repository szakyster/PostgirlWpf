using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Converters;

public class BodyTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not BodyType current)
            return Visibility.Collapsed;

        if (parameter is not string targetName)
            return Visibility.Collapsed;

        if (!Enum.TryParse<BodyType>(targetName, out var target))
            return Visibility.Collapsed;

        return current == target
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}