using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PowerTools.Core.Converters
{
    public class MultiValueToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return false;
            }

            if (values.Any(p => p == null || p == DependencyProperty.UnsetValue))
            {
                return false;
            }

            if (parameter == null)
            {
                return values.All(p => System.Convert.ToBoolean(p.ToString()));
            }

            return values.All(p => p.ToString() == parameter.ToString());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
