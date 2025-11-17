using PowerTools.Core.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PowerTools.Converters
{
    public class ModuleHeaderNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var module = value as ToolModule;
            if (!string.IsNullOrEmpty(module?.Name))
            {
                return $"PowerTools - {module.Name}";
            }
            else
            {
                return "PowerTools";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
