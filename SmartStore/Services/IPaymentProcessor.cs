using SmartStorePOS.Models;
using SmartStorePOS.Views;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Interface xử lý thanh toán
    /// </summary>
    public interface IPaymentProcessor
    {
        /// <summary>
        /// Xử lý thanh toán cho đơn hàng
        /// </summary>
        /// <param name="order">Đơn hàng cần thanh toán</param>
        /// <param name="paymentMethod">Phương thức thanh toán</param>
        /// <returns>Kết quả thanh toán</returns>
        Task<PaymentResponse> ProcessPayment(Order order, PaymentMethodWindow.PaymentMethod paymentMethod);
    }
}
