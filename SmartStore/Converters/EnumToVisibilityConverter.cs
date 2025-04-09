using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartStorePOS.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            var currentValue = value.ToString();
            var parameterValue = parameter.ToString();

            return currentValue.Equals(parameterValue, StringComparison.InvariantCultureIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
