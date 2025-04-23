using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using SmartStorePOS.Services;
using SmartStorePOS.Views;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SmartStorePOS.ViewModels
{
    public class MembershipCardViewModel : ViewModelBase, IDisposable
    {
        private readonly IDialogService _dialogService;
        private StringBuilder _cardDataBuffer = new StringBuilder();
        private string _statusMessage = "Vui lòng quẹt thẻ hoặc nhập mã thẻ...";
        private bool _isReading = true;
        private Order _order;
        private PaymentResponse _paymentResponse;
        private readonly IApiService _apiService;

        public Order Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public PaymentResponse PaymentResponse
        {
            get => _paymentResponse;
            set => SetProperty(ref _paymentResponse, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsReading
        {
            get => _isReading;
            set => SetProperty(ref _isReading, value);
        }

        public ICommand CancelCommand { get; }

        public MembershipCardViewModel(IDialogService dialogService, IApiService apiService)
        {
            _apiService = apiService;
            _dialogService = dialogService;
            CancelCommand = new RelayCommand(_ => CancelReading());
            InitializeKeyboardInput();
        }

        private void InitializeKeyboardInput()
        {
            // Find the window associated with this view model and attach key event handler
            var a = App.Current.Windows.OfType<Window>().FirstOrDefault(x => x.Title == "Thanh toán bằng thẻ thành viên");
            if (a is Window window)
            {
                window.KeyDown += Window_KeyDown;
                window.Closed += Window_Closed; // Ensure cleanup
            }
            else
            {
                StatusMessage = "Không thể gắn sự kiện bàn phím.";
                IsReading = false;
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsReading) return;

            try
            {
                // Handle numeric keys and Enter
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    // Convert Key to digit (e.g., Key.D0 -> '0')
                    char digit = (char)('0' + (e.Key - Key.D0));
                    _cardDataBuffer.Append(digit);
                    StatusMessage = "Đang nhập mã thẻ...";
                }
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    // Handle numpad keys
                    char digit = (char)('0' + (e.Key - Key.NumPad0));
                    _cardDataBuffer.Append(digit);
                    StatusMessage = "Đang nhập mã thẻ...";
                }
                else if (e.Key == Key.Enter)
                {
                    // Process card number when Enter is pressed
                    string cardNumber = _cardDataBuffer.ToString().Trim();
                    _cardDataBuffer.Clear();
                    await ProcessCardNumber(cardNumber);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi khi đọc mã thẻ: {ex.Message}";
            }
        }

        private async Task ProcessCardNumber(string cardNumber)
        {
            try
            {
                // Remove any non-numeric characters (in case of unexpected input)
                cardNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
                cardNumber = "0002008235";

                if (string.IsNullOrEmpty(cardNumber))
                {
                    StatusMessage = "Không thể đọc được mã thẻ.";
                    return;
                }

                // TODO: Validate card number and process payment
                //StatusMessage = $"Đã đọc thẻ thành công: {cardNumber}";
                IsReading = false;

                StatusMessage = $"Đang thanh toán ...";

                var uuid = UuidConverter.ToUuidV4(cardNumber);
                PaymentResponse = await _apiService.CreatePaymentAsync(new CreatePaymentRequest
                {
                    OrderId = Guid.Parse(Order.OrderId),
                    CardId = uuid,
                });

                if (PaymentResponse == null)
                {
                    StatusMessage = "Đã xảy ra lỗi khi thanh toán.";
                    return;
                }

                // Close the window with success
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                //StatusMessage = $"Lỗi khi đọc mã thẻ: {ex.Message}";
                StatusMessage = "Đã xảy ra lỗi khi thanh toán.";
                return;
            }
        }

        private void CancelReading()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool result)
        {
            if (GetCurrentWindow() is Window window)
            {
                window.DialogResult = result;
                window.Close();
            }
        }

        private Window GetCurrentWindow()
        {
            return App.Current.Windows.OfType<Window>().FirstOrDefault(w => w.Name == nameof(MembershipCardWindow));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup event handlers
            if (sender is Window window)
            {
                window.KeyDown -= Window_KeyDown;
                window.Closed -= Window_Closed;
            }
        }

        public void Dispose()
        {
            // Remove event handlers from window
            if (GetCurrentWindow() is Window window)
            {
                window.KeyDown -= Window_KeyDown;
                window.Closed -= Window_Closed;
            }
            _cardDataBuffer.Clear();
        }
    }
}