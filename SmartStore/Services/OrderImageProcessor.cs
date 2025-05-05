using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using System.Configuration;
using System.Windows.Media.Imaging;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Triển khai IOrderImageProcessor
    /// </summary>
    public class OrderImageProcessor : IOrderImageProcessor
    {
        private readonly IApiService _apiService;

        // URL của các hình ảnh
        private string _imageUrl1;
        private string _imageUrl2;
        private string _imageUrl3;

        public string ImageUrl1 => _imageUrl1;
        public string ImageUrl2 => _imageUrl2;
        public string ImageUrl3 => _imageUrl3;

        public OrderImageProcessor(IApiService apiService)
        {
            _apiService = apiService;
        }

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

                    // Gọi API để upload hình ảnh
                    if (image1Path != null)
                    {
                        var uploadResponse1 = await _apiService.UploadImageAsync(image1Path);
                        _imageUrl1 = uploadResponse1.image_url;
                    }

                    if (image2Path != null)
                    {
                        var uploadResponse2 = await _apiService.UploadImageAsync(image2Path);
                        _imageUrl2 = uploadResponse2.image_url;
                    }

                    if (image3Path != null)
                    {
                        var uploadResponse3 = await _apiService.UploadImageAsync(image3Path);
                        _imageUrl3 = uploadResponse3.image_url;
                    }
                }
                else
                {
                    // Chế độ Dev - Sử dụng URL mẫu từ cấu hình
                    _imageUrl1 = ConfigurationManager.AppSettings["Img1Url"];
                    _imageUrl2 = ConfigurationManager.AppSettings["Img2Url"];
                    _imageUrl3 = ConfigurationManager.AppSettings["Img3Url"];
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

        /// <summary>
        /// Cập nhật đơn hàng khi có thay đổi
        /// </summary>
        public async Task<Order> UpdateOrder(string orderId, List<OrderItem> orderItems)
        {
            try
            {
                var updateOrderRequest = new UpdateOrderWrongRequest
                {
                    OldOrderId = orderId,
                    Items = [.. orderItems.Select(x => new UpdateOrderWrongItems
                    {
                        ProductId = x.ProductId,
                        Count = x.Count,
                    })]
                };

                var updatedOrder = await _apiService.UpdateOrderWrongAsync(updateOrderRequest);
                return updatedOrder;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật đơn hàng: {ex.Message}", ex);
            }
        }
    }
}
