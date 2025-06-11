using PowerTools.Core.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PowerTools.Converters
{
    public class ToolModuleVersionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var module = (ToolModule)value;
            return $"{module.Version} ~ {module.LatestVersion}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
