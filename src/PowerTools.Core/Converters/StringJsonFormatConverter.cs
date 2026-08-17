using System;
using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace PowerTools.Core.Converters
{
    public class StringJsonFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    return JValue.Parse(value.ToString()).ToString(Newtonsoft.Json.Formatting.Indented);
                }
                catch
                {
                    return value;
                }
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    return JValue.Parse(value.ToString()).ToString(Newtonsoft.Json.Formatting.None);
                }
                catch
                {
                    return value;
                }
            }
            else
            {
                return value;
            }
        }
    }
}
