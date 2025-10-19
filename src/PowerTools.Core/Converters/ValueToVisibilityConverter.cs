using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PowerTools.Core.Converters
{
    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (parameter == null)
            {
                return Visibility.Visible;
            }

            var valueStr = value.ToString();
            return valueStr == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
