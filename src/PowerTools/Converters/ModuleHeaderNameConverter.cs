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
            var moduleName = value as string;
            return $"Module: {moduleName}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
