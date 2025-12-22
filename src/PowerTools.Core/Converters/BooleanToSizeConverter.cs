using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PowerTools.Core.Converters
{
    public class BooleanToGridSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool?)value;
            var param = parameter == null ? string.Empty : parameter.ToString();
            if (boolValue.GetValueOrDefault())
            {
                return $"{param}";
            }

            return "Auto";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
