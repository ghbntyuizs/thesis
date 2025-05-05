namespace SmartStorePOS.Helpers
{
    public class ImageHelper
    {
        /// <summary>
        /// Lưu ảnh vào thư mục temp
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string SaveImageToTempFile(System.Windows.Media.Imaging.BitmapSource image)
        {
            try
            {
                // Tạo tên file duy nhất sử dụng Guid
                string fileName = $"{Guid.NewGuid()}.jpg";
                string tempPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    fileName);

                // Lưu ảnh vào file tạm
                using (var fileStream = new System.IO.FileStream(tempPath, System.IO.FileMode.Create))
                {
                    var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

                return tempPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Xóa file tạm
        /// </summary>
        public static void DeleteTempFile(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting temp file: {ex.Message}");
            }
        }
    }
}
