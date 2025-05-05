using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartStorePOS.Converters
{
    /// <summary>
    /// Chuyển đổi giá trị null/empty thành Visibility
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = true;

            if (value == null)
                isNullOrEmpty = true;
            else if (value is string str)
                isNullOrEmpty = string.IsNullOrEmpty(str);
            else
                isNullOrEmpty = false;

            // Mặc định: null/empty -> Visible, ngược lại -> Collapsed
            // Nếu có parameter và parameter là "Inverse" thì ngược lại: null/empty -> Collapsed, ngược lại -> Visible
            bool isInverse = parameter != null && parameter.ToString() == "Inverse";

            if (isInverse)
                return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
            else
                return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
