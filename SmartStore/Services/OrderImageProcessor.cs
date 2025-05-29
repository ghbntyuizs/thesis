using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using System.Configuration;
using System.IO;
using System.Windows.Media.Imaging;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Triển khai IOrderImageProcessor
    /// </summary>
    public class OrderImageProcessor(IApiService apiService) : IOrderImageProcessor
    {
        private readonly IApiService _apiService = apiService;

        // URL của các hình ảnh
        private string _imageUrl1;
        private string _imageUrl2;
        private string _imageUrl3;

        public string ImageUrl1 => _imageUrl1;
        public string ImageUrl2 => _imageUrl2;
        public string ImageUrl3 => _imageUrl3;

        /// <summary>
        /// Xử lý hình ảnh và tạo đơn hàng
        /// </summary>
        public async Task<Order> ProcessOrderFromImages(BitmapSource image1, BitmapSource image2, BitmapSource image3)
        {
            try
            {
                // Reset URL trước khi upload
                _imageUrl1 = string.Empty;
                _imageUrl2 = null;
                _imageUrl3 = null;

                if (ConfigurationManager.AppSettings["Env"] != "Dev")
                {
                    // Lưu hình ảnh vào thư mục tạm
                    var image1Path = image1 != null ? ImageHelper.SaveImageToTempFile(image1) : null;
                    var image2Path = image2 != null ? ImageHelper.SaveImageToTempFile(image2) : null;
                    var image3Path = image3 != null ? ImageHelper.SaveImageToTempFile(image3) : null;

                    // tạo mảng imagePath
                    var imagePaths = new[] { image1Path, image2Path, image3Path };

                    var uploadTasks = new List<Task>();

                    // Gọi API để upload hình ảnh
                    if (image1Path != null)
                    {
                        uploadTasks.Add(Task.Run(async () =>
                        {
                            var uploadResponse1 = await _apiService.UploadImageAsync(image1Path);
                            _imageUrl1 = uploadResponse1.image_url;
                            ImageHelper.DeleteTempFile(image1Path); // Xóa file tạm sau khi upload
                        }));
                    }

                    if (image2Path != null)
                    {
                        uploadTasks.Add(Task.Run(async () =>
                        {
                            var uploadResponse2 = await _apiService.UploadImageAsync(image2Path);
                            _imageUrl2 = uploadResponse2.image_url;
                            ImageHelper.DeleteTempFile(image2Path); // Xóa file tạm sau khi upload
                        }));
                    }

                    if (image3Path != null)
                    {
                        uploadTasks.Add(Task.Run(async () =>
                        {
                            var uploadResponse3 = await _apiService.UploadImageAsync(image3Path);
                            _imageUrl3 = uploadResponse3.image_url;
                            ImageHelper.DeleteTempFile(image3Path); // Xóa file tạm sau khi upload
                        }));
                    }

                    // Chờ tất cả các tác vụ upload hoàn thành
                    await Task.WhenAll(uploadTasks);
                }
                else
                {
                    // Chế độ Dev - Sử dụng URL mẫu từ cấu hình
                    _imageUrl1 = ConfigurationManager.AppSettings["Img1Url"];
                    _imageUrl2 = ConfigurationManager.AppSettings["Img2Url"];
                    _imageUrl3 = ConfigurationManager.AppSettings["Img3Url"];
                }

                // xem nếu config là mock từ file thì 
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MockOrderFilePath"])) 
                {
                    string orderString = File.ReadAllText(ConfigurationManager.AppSettings["MockOrderFilePath"]);
                    return System.Text.Json.JsonSerializer.Deserialize<Order>(orderString, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                // Gọi API để tạo đơn hàng
                var order = await _apiService.CreateOrderAsync(new CreateOrderRequest
                {
                    Image1 = _imageUrl1,
                    Image2 = _imageUrl2 ?? _imageUrl1,
                    Image3 = _imageUrl3 ?? _imageUrl1,
                });

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xử lý hình ảnh: {ex.Message}", ex);
            }
        }
    }
}
