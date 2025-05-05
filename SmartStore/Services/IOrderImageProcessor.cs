using SmartStorePOS.Models;
using System.Windows.Media.Imaging;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Interface xử lý hình ảnh và tạo đơn hàng từ hình ảnh
    /// </summary>
    public interface IOrderImageProcessor
    {
        /// <summary>
        /// Xử lý hình ảnh và tạo đơn hàng
        /// </summary>
        /// <param name="image1">Hình ảnh từ camera 1</param>
        /// <param name="image2">Hình ảnh từ camera 2</param>
        /// <param name="image3">Hình ảnh từ camera 3</param>
        /// <returns>Đơn hàng đã tạo</returns>
        Task<Order> ProcessOrderFromImages(BitmapSource image1, BitmapSource image2, BitmapSource image3);

        /// <summary>
        /// Lấy URL của hình ảnh 1
        /// </summary>
        string ImageUrl1 { get; }

        /// <summary>
        /// Lấy URL của hình ảnh 2
        /// </summary>
        string ImageUrl2 { get; }

        /// <summary>
        /// Lấy URL của hình ảnh 3
        /// </summary>
        string ImageUrl3 { get; }

        /// <summary>
        /// Cập nhật đơn hàng khi có thay đổi
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        /// <param name="orderItems">Danh sách các mặt hàng mới</param>
        /// <returns>Đơn hàng đã cập nhật</returns>
        Task<Order> UpdateOrder(string orderId, System.Collections.Generic.List<OrderItem> orderItems);
    }
}
