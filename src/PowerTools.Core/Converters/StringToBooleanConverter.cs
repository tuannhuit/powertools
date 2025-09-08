using System;
using System.Globalization;
using System.Windows.Data;

namespace PowerTools.Core.Converters
{
    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == string.Empty) return false;
            if (parameter == null) return true;

            if (value.ToString() == parameter.ToString()) return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
