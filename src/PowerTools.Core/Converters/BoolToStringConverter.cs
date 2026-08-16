using System;
using System.Globalization;
using System.Windows.Data;

namespace PowerTools.Core.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return value;

            var strValue = value.ToString();
            var paramString = parameter == null ? string.Empty : parameter.ToString();

            var values = paramString.Split(":");

            return strValue.ToLower() == values[0].ToLower() ? values[1] : values[2];
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
