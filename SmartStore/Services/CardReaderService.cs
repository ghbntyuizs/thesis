using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Triển khai dịch vụ đọc thẻ tự động
    /// </summary>
    public class CardReaderService : ICardReaderService
    {
        private readonly StringBuilder _cardDataBuffer = new StringBuilder();
        private bool _isListening = false;
        private Window _mainWindow;

        public event EventHandler<string> CardScanned;
        public event EventHandler<Exception> CardReadError;

        public bool IsListening => _isListening;

        /// <summary>
        /// Constructor mặc định
        /// </summary>
        public CardReaderService()
        {
        }

        /// <summary>
        /// Bắt đầu lắng nghe thẻ
        /// </summary>
        public async Task StartListeningAsync()
        {
            if (_isListening)
                return;

            try
            {
                // Lấy cửa sổ chính của ứng dụng để lắng nghe sự kiện KeyDown
                _mainWindow = Application.Current.MainWindow;
                if (_mainWindow != null)
                {
                    // Đăng ký sự kiện KeyDown
                    _mainWindow.KeyDown += MainWindow_KeyDown;
                    _isListening = true;
                }
                else
                {
                    throw new Exception("Không thể tìm thấy cửa sổ chính của ứng dụng.");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                CardReadError?.Invoke(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Dừng lắng nghe thẻ
        /// </summary>
        public async Task StopListeningAsync()
        {
            if (!_isListening)
                return;

            try
            {
                if (_mainWindow != null)
                {
                    _mainWindow.KeyDown -= MainWindow_KeyDown;
                }

                _isListening = false;
                _cardDataBuffer.Clear();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                CardReadError?.Invoke(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Xử lý sự kiện KeyDown
        /// </summary>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Chỉ xử lý khi đang lắng nghe
                if (!_isListening)
                    return;

                // Chỉ phản ứng với phím số
                if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                {
                    char digit;
                    if (e.Key >= Key.D0 && e.Key <= Key.D9)
                    {
                        digit = (char)('0' + (e.Key - Key.D0));
                    }
                    else
                    {
                        digit = (char)('0' + (e.Key - Key.NumPad0));
                    }

                    _cardDataBuffer.Append(digit);
                }
                else if (e.Key == Key.OemQuestion)
                {
                    // Xử lý khi Enter được nhấn
                    ProcessCardNumber();
                }
            }
            catch (Exception ex)
            {
                CardReadError?.Invoke(this, ex);
                _cardDataBuffer.Clear();
            }
        }

        /// <summary>
        /// Xử lý mã thẻ
        /// </summary>
        private void ProcessCardNumber()
        {
            string cardNumber = _cardDataBuffer.ToString().Trim();
            _cardDataBuffer.Clear();

            if (!string.IsNullOrEmpty(cardNumber))
            {
                // Kích hoạt sự kiện thẻ được quét
                CardScanned?.Invoke(this, cardNumber);
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            if (_mainWindow != null)
            {
                _mainWindow.KeyDown -= MainWindow_KeyDown;
                _mainWindow = null;
            }

            _cardDataBuffer.Clear();
            _isListening = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FocusMainWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Focus();
            }
        }
    }
}
