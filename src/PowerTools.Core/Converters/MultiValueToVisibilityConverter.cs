using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PowerTools.Core.Converters
{
    public class MultiValueToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return Visibility.Collapsed;
            }

            if (values.Any(p => p == null || p == DependencyProperty.UnsetValue))
            {
                return Visibility.Collapsed;
            }

            if (parameter == null)
            {
                return Visibility.Visible;
            }

            return values.All(p => p.ToString() == parameter.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
