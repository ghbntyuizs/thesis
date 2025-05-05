using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SmartStorePOS.Converters
{
    /// <summary>
    /// Chuyển đổi chuỗi Base64 thành hình ảnh
    /// </summary>
    public class Base64ToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string base64String) || string.IsNullOrWhiteSpace(base64String))
                return null;

            try
            {
                // Loại bỏ header nếu có
                if (base64String.StartsWith("data:image"))
                {
                    base64String = base64String.Substring(base64String.IndexOf(",") + 1);
                }

                // Chuyển đổi Base64 thành mảng byte
                byte[] imageBytes = System.Convert.FromBase64String(base64String);

                // Tạo BitmapImage từ mảng byte
                BitmapImage image = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze(); // Để tránh lỗi cross-thread
                }

                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
