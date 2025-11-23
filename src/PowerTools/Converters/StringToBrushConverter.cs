using PowerTools.Core.SharedServices;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PowerTools.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _iconColor = value?.ToString();
            if (string.IsNullOrEmpty(_iconColor) || string.IsNullOrWhiteSpace(_iconColor))
            {
                return Brushes.Black;
            }

            try
            {
                BrushConverter converter = new BrushConverter();

                // Attempt to convert the string to a SolidColorBrush
                // The string can be a named color (e.g., "Red"), a hex code (e.g., "#FF0000"),
                // or an ARGB hex code (e.g., "#FFFF0000").
                return (SolidColorBrush)converter.ConvertFromString(_iconColor);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error($"Failed to convert IconColorBrush", e);
                return Brushes.Black;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
