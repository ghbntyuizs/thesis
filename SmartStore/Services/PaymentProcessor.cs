using SmartStorePOS.Models;
using SmartStorePOS.ViewModels;
using SmartStorePOS.Views;
using System.Windows;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Triển khai IPaymentProcessor
    /// </summary>
    public class PaymentProcessor : IPaymentProcessor
    {
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;

        public PaymentProcessor(IApiService apiService, IDialogService dialogService)
        {
            _apiService = apiService;
            _dialogService = dialogService;
        }

        /// <summary>
        /// Xử lý thanh toán cho đơn hàng
        /// </summary>
        public async Task<PaymentResponse> ProcessPayment(Order order, PaymentMethodWindow.PaymentMethod paymentMethod)
        {
            try
            {
                PaymentResponse response = null;

                switch (paymentMethod)
                {
                    case PaymentMethodWindow.PaymentMethod.QRCode:
                        // Hiển thị mã QR để thanh toán
                        var qrWindow = new QRCodeWindow($"aistore://payment/{order.OrderId}");
                        qrWindow.ShowDialog();

                        // Đối với QR Code, chúng ta không biết kết quả thanh toán ngay lập tức
                        // Có thể sử dụng WebSocket để nhận thông báo khi thanh toán hoàn tất
                        response = new PaymentResponse
                        {
                            //OrderId = order.OrderId,
                            //Amount = order.Total,
                            //Status = "PENDING",
                            //PaymentMethod = "QR_CODE"
                        };
                        break;

                    case PaymentMethodWindow.PaymentMethod.MembershipCard:
                        // Xử lý thanh toán bằng thẻ thành viên
                        var membershipWindow = _dialogService.GetService<MembershipCardWindow>();
                        MembershipCardViewModel membershipViewModel = membershipWindow.DataContext as MembershipCardViewModel;
                        membershipViewModel.Order = order;
                        membershipWindow.Owner = Application.Current.MainWindow;

                        // Hiển thị cửa sổ thanh toán thẻ thành viên
                        membershipWindow.ShowDialog();

                        // Lấy kết quả thanh toán từ ViewModel
                        response = membershipViewModel.PaymentResponse;
                        membershipViewModel.Dispose();
                        break;
                }

                //if (response != null && response.Status == "SUCCESS")
                //{
                //    // Gọi API để cập nhật trạng thái thanh toán của đơn hàng
                //    await _apiService.CreatePaymentAsync(new CreatePaymentRequest
                //    {
                //        //OrderId = order.OrderId,
                //        //Amount = order.Total,
                //        //PaymentMethod = response.PaymentMethod
                //    });
                //}

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xử lý thanh toán: {ex.Message}", ex);
            }
        }
    }
}
