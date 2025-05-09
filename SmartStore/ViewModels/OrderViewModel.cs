using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using SmartStorePOS.Services;
using SmartStorePOS.ViewModels.States;
using SmartStorePOS.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SmartStorePOS.ViewModels
{
    /// <summary>
    /// ViewModel quản lý đơn hàng
    /// </summary>
    public class OrderViewModel : ViewModelBase, IDisposable
    {
        #region Services

        // Các service được inject từ DI container
        private readonly INavigationService _navigationService;
        public readonly IDialogService DialogService;
        private readonly IWebSocketService _webSocketService;
        public readonly ICameraService CameraService;
        private readonly IOrderImageProcessor _orderImageProcessor;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly ICardReaderService _cardReaderService;
        private readonly IApiService _apiService;

        // Quản lý state
        public readonly OrderStateManager StateManager;

        #endregion

        #region Properties

        private string _qrCodeUrl;
        private bool _isLoading;
        private string _errorMessage;
        private string _captureButtonText = "Bắt đầu chụp";
        private bool _isCameraRunning = false;
        private string _overlayText;
        private string _orderText = "Xử lý đơn hàng";
        private bool _isShowOrderButton = true;
        private bool _isShowCancelButton = false;
        private bool _isCaptured = false;
        private bool _isCardReaderActive = false;
        private bool _isOrderProcessed = false;
        private string _cardReaderStatus = "Đang chờ quét thẻ...";

        private string _imageUrl1;
        private string _imageUrl2;
        private string _imageUrl3;
        private bool _isImageUploaded = false;
        private string _boxedImage;

        private Order _order;
        private ObservableCollection<OrderItem> _items;

        private BitmapSource _streamImage1;
        private BitmapSource _streamImage2;
        private BitmapSource _streamImage3;

        private BitmapSource _capturedImage1;
        private BitmapSource _capturedImage2;
        private BitmapSource _capturedImage3;

        private decimal _total;

        public Action? FocusMainWindow;

        public Order Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public string QrCodeUrl
        {
            get => _qrCodeUrl;
            set => SetProperty(ref _qrCodeUrl, value);
        }

        public BitmapSource StreamImage1
        {
            get => _streamImage1;
            set => SetProperty(ref _streamImage1, value);
        }

        public BitmapSource StreamImage2
        {
            get => _streamImage2;
            set => SetProperty(ref _streamImage2, value);
        }

        public BitmapSource StreamImage3
        {
            get => _streamImage3;
            set => SetProperty(ref _streamImage3, value);
        }

        public BitmapSource CapturedImage1
        {
            get => _capturedImage1;
            set => SetProperty(ref _capturedImage1, value);
        }

        public BitmapSource CapturedImage2
        {
            get => _capturedImage2;
            set => SetProperty(ref _capturedImage2, value);
        }

        public BitmapSource CapturedImage3
        {
            get => _capturedImage3;
            set => SetProperty(ref _capturedImage3, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsCameraRunning
        {
            get => _isCameraRunning;
            set => SetProperty(ref _isCameraRunning, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string CaptureButtonText
        {
            get => _captureButtonText;
            set => SetProperty(ref _captureButtonText, value);
        }

        public ObservableCollection<OrderItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public string OverlayText
        {
            get => _overlayText;
            set => SetProperty(ref _overlayText, value);
        }

        public string OrderText
        {
            get => _orderText;
            set => SetProperty(ref _orderText, value);
        }

        public bool IsShowOrderButton
        {
            get => _isShowOrderButton;
            set => SetProperty(ref _isShowOrderButton, value);
        }

        public bool IsShowCancelButton
        {
            get => _isShowCancelButton;
            set => SetProperty(ref _isShowCancelButton, value);
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public string ImageUrl1
        {
            get => _imageUrl1;
            set => SetProperty(ref _imageUrl1, value);
        }

        public string ImageUrl2
        {
            get => _imageUrl2;
            set => SetProperty(ref _imageUrl2, value);
        }

        public string ImageUrl3
        {
            get => _imageUrl3;
            set => SetProperty(ref _imageUrl3, value);
        }

        public bool IsImageUploaded
        {
            get => _isImageUploaded;
            set => SetProperty(ref _isImageUploaded, value);
        }

        public string BoxedImage
        {
            get => _boxedImage;
            set => SetProperty(ref _boxedImage, value);
        }

        public bool IsCaptured
        {
            get => _isCaptured;
            set => SetProperty(ref _isCaptured, value);
        }

        public bool IsCardReaderActive
        {
            get => _isCardReaderActive;
            set => SetProperty(ref _isCardReaderActive, value);
        }

        public bool IsOrderProcessed
        {
            get => _isOrderProcessed;
            set => SetProperty(ref _isOrderProcessed, value);
        }

        public string CardReaderStatus
        {
            get => _cardReaderStatus;
            set => SetProperty(ref _cardReaderStatus, value);
        }
        #endregion

        #region Commands

        public ICommand NewOrderCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CaptureImagesCommand { get; }
        //public ICommand ProcessOrderCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand HelpCommand { get; }
        public ICommand CopyImageUrl1Command { get; }
        public ICommand CopyImageUrl2Command { get; }
        public ICommand CopyImageUrl3Command { get; }

        // Command thanh toán
        public ICommand QRCodePaymentCommand { get; }
        //public ICommand MembershipCardPaymentCommand { get; }
        //public ICommand PaymentCommand { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public OrderViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IWebSocketService webSocketService,
            ICameraService cameraService,
            IOrderImageProcessor orderImageProcessor,
            IPaymentProcessor paymentProcessor,
            ICardReaderService cardReaderService,
            IApiService apiService)
        {
            _navigationService = navigationService;
            DialogService = dialogService;
            _webSocketService = webSocketService;
            CameraService = cameraService;
            _orderImageProcessor = orderImageProcessor;
            _paymentProcessor = paymentProcessor;
            _cardReaderService = cardReaderService;
            _apiService = apiService;

            // Khởi tạo StateManager
            StateManager = new OrderStateManager(this);

            // Đăng ký sự kiện đọc thẻ
            _cardReaderService.CardScanned += OnCardScanned;
            _cardReaderService.CardReadError += OnCardReadError;

            // Đăng ký các sự kiện camera
            CameraService.CameraInitialized += OnCameraInitialized;
            CameraService.CameraError += OnCameraError;
            CameraService.FrameUpdated += OnCameraFrameUpdated;

            // Khởi tạo WebSocket
            //InitializeWebSocket();

            // Khởi tạo collections
            Items = new ObservableCollection<OrderItem>();

            // Khởi tạo commands
            NewOrderCommand = new RelayCommand(_ => _navigationService.NavigateTo<MainViewModel>());
            LogoutCommand = new RelayCommand(_ => _navigationService.NavigateTo<LoginViewModel>());
            CaptureImagesCommand = new RelayCommand(async _ => await HandleCaptureImagesCommand());
            //ProcessOrderCommand = new RelayCommand(async _ => await HandleProcessOrderCommand());
            CancelCommand = new RelayCommand(_ => HandleCancelCommand());
            HelpCommand = new RelayCommand(_ => DialogService.ShowInfoDialog("Thông báo", "Tính năng đang được phát triển."));

            // Các lệnh copy URL
            CopyImageUrl1Command = new RelayCommand(_ => CopyTextToClipboard(ImageUrl1));
            CopyImageUrl2Command = new RelayCommand(_ => CopyTextToClipboard(ImageUrl2));
            CopyImageUrl3Command = new RelayCommand(_ => CopyTextToClipboard(ImageUrl3));

            // Command thanh toán
            QRCodePaymentCommand = new RelayCommand(_ => HandleQRCodePayment());
            //QRCodePaymentCommand = new RelayCommand(_ => DialogService.ShowInfoDialog("Thông báo", "Tính năng thanh toán QR Code đang được phát triển."));
            //MembershipCardPaymentCommand = new RelayCommand(async _ => await HandleMembershipCardPayment());

            // Bắt đầu lắng nghe thẻ ngay khi khởi tạo
            // StartAutoCardReadingAsync().ConfigureAwait(false);

            // Khởi tạo đơn hàng
            InitializeOrder();
        }

        /// <summary>
        /// Xử lý sự kiện camera được khởi tạo
        /// </summary>
        private void OnCameraInitialized(object sender, EventArgs e)
        {
            // Chuyển sang trạng thái CameraActiveState
            Application.Current.Dispatcher.Invoke(() =>
            {
                StateManager.TransitionTo(new CameraActiveState());
            });
        }

        /// <summary>
        /// Xử lý sự kiện lỗi camera
        /// </summary>
        private void OnCameraError(object sender, Exception e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorMessage = $"Lỗi camera: {e.Message}";
                StateManager.TransitionTo(new InitialState());
            });
        }

        /// <summary>
        /// Xử lý sự kiện frame được cập nhật
        /// </summary>
        private void OnCameraFrameUpdated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StreamImage1 = CameraService.Image1;
                StreamImage2 = CameraService.Image2;
                StreamImage3 = CameraService.Image3;
            });
        }

        #region Command Handlers

        #region Card Reader Handlers

        /// <summary>
        /// Xử lý sự kiện thẻ được quét
        /// </summary>
        private async void OnCardScanned(object sender, string cardNumber)
        {
            try
            {
                // Chỉ xử lý khi ở trạng thái OrderCreatedState và chưa xử lý đơn hàng
                if (StateManager.GetCurrentState().GetStateName() == "OrderCreated" && !IsOrderProcessed)
                {
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        CardReaderStatus = "Đã phát hiện thẻ, đang xử lý...";
                        await HandleProcessOrderByCard(cardNumber);
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi xử lý thẻ: {ex.Message}";
                CardReaderStatus = "Lỗi khi đọc thẻ";
            }
        }

        /// <summary>
        /// Xử lý sự kiện lỗi đọc thẻ
        /// </summary>
        private void OnCardReadError(object sender, Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorMessage = $"Lỗi đọc thẻ: {ex.Message}";
                CardReaderStatus = "Lỗi khi đọc thẻ";
            });
        }

        /// <summary>
        /// Xử lý đơn hàng khi thẻ được quét
        /// </summary>
        private async Task HandleProcessOrderByCard(string cardNumber)
        {
            if (!ValidateOrder())
                return;

            IsLoading = true;
            CardReaderStatus = OverlayText = "Đang thực hiện thanh toán...";
            StateManager.TransitionTo(new PaymentProcessingState());

            try
            {
                var paymentResponse = await _apiService.CreatePaymentAsync(new CreatePaymentRequest
                {
                    OrderId = Guid.Parse(Order.OrderId),
                    CardId = UuidConverter.ToUuidV4(cardNumber),
                });

                IsLoading = false;
                if (paymentResponse.Status != "SUCCESS")
                {
                    string msg = paymentResponse.Msg ?? "Thanh toán không thành công, vui lòng thử lại.";
                    if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        msg = "Thẻ không tồn tại trong hệ thống!";
                    }

                    if (msg.Contains("not enough", StringComparison.OrdinalIgnoreCase))
                    {
                        msg = "Số dư tài khoản không đủ để thực hiện giao dịch này";
                    }

                    CardReaderStatus = OverlayText = msg;
                    DialogService.ShowInfoDialog("Thông báo", msg);
                    StateManager.TransitionTo(new OrderCreatedState());
                    return;
                }

                CardReaderStatus = "Thanh toán thành công!";
                DialogService.ShowInfoDialog("Thông báo", $"Đã thanh toán thành công số tiền {paymentResponse.Amount:N0} VNĐ. Số dư thẻ còn lại: {paymentResponse.RemainBalance:N0} VNĐ.");
                StateManager.TransitionTo(new PaymentCompletedState());
            }
            catch (Exception ex)
            {
                CardReaderStatus = OverlayText = ex.Message;
                DialogService.ShowInfoDialog("Thông báo", $"{ex.Message}");
                StateManager.TransitionTo(new OrderCreatedState());
            }
        }

        /// <summary>
        /// Xử lý khi người dùng ấn nút thanh toán QR Code
        /// </summary>
        private void HandleQRCodePayment()
        {
            if (!ValidateOrder())
                return;

            IsLoading = true;
            OverlayText = "Đang thực hiện thanh toán...";
            StateManager.TransitionTo(new PaymentProcessingState());

            try
            {
                var qrWindow = new QRCodeWindow($"aistore://payment/{Order.OrderId}");
                var resullt = qrWindow.ShowDialog();
                if (resullt == true)
                {
                    qrWindow.Close();
                }

                FocusMainWindow?.Invoke();

                //"Thanh toán thành công!";
                //DialogService.ShowInfoDialog("Thông báo", $"Đã thanh toán thành công số tiền {paymentResponse.Amount:N0} VNĐ. Số dư thẻ còn lại: {paymentResponse.RemainBalance:N0} VNĐ.");
                //StateManager.TransitionTo(new PaymentCompletedState());
            }
            catch (Exception ex)
            {
                OverlayText = ex.Message;
                DialogService.ShowInfoDialog("Thông báo", $"{ex.Message}");
                StateManager.TransitionTo(new OrderCreatedState());
            }
            finally
            {
                IsLoading = false;
                IsShowOrderButton = true;
                OverlayText = "";
                StateManager.TransitionTo(new OrderCreatedState());
            }
        }

        /// <summary>
        /// Xử lý khi người dùng ấn nút thanh toán thẻ thành viên
        /// </summary>
        //private async Task HandleMembershipCardPayment()
        //{
        //    if (!ValidateOrder())
        //        return;

        //    IsLoading = true;
        //    OverlayText = "Đang thực hiện thanh toán qua thẻ thành viên...";
        //    StateManager.TransitionTo(new PaymentProcessingState());

        //    await ProcessPayment(PaymentMethodWindow.PaymentMethod.MembershipCard);
        //}

        /// <summary>
        /// Xử lý khi người dùng ấn nút chụp ảnh
        /// </summary>
        private async Task HandleCaptureImagesCommand()
        {
            await StateManager.GetCurrentState().HandleCaptureImages(this);
        }

        /// <summary>
        /// Xử lý khi người dùng ấn nút xử lý đơn hàng
        /// </summary>
        //private async Task HandleProcessOrderCommand()
        //{
        //    await StateManager.GetCurrentState().HandleProcessOrder(this);
        //}

        /// <summary>
        /// Xử lý khi người dùng ấn nút hủy
        /// </summary>
        private void HandleCancelCommand()
        {
            StateManager.GetCurrentState().HandleCancel(this);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hiển thị thông báo tính năng đang phát triển
        /// </summary>
        //private void ShowFeatureInDevelopmentMessage(string featureName)
        //{
        //    DialogService.ShowInfoDialog("Thông báo", $"Tính năng {featureName} đang được phát triển.");
        //}

        /// <summary>
        /// Copy URL vào clipboard
        /// </summary>
        private void CopyTextToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Clipboard.SetText(text);
                    DialogService.ShowInfoDialog("Thông báo", "Đã copy URL vào clipboard!");
                }
                catch (Exception ex)
                {
                    DialogService.ShowInfoDialog("Lỗi", $"Không thể copy URL: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Khởi tạo đơn hàng mới
        /// </summary>
        public void InitializeOrder()
        {
            Order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                Status = "CREATED",
                CreatedDate = DateTime.Now,
                Items = new System.Collections.Generic.List<OrderItem>(),
            };
        }

        /// <summary>
        /// Xử lý hình ảnh và tạo đơn hàng từ hình ảnh
        /// </summary>
        public async Task HandleLoadOrderByImage()
        {
            try
            {
                // Sử dụng OrderImageProcessor để xử lý hình ảnh và tạo đơn hàng
                var order = await _orderImageProcessor.ProcessOrderFromImages(
                    CapturedImage1,
                    CapturedImage2,
                    CapturedImage3);

                // Cập nhật URL hình ảnh từ processor
                ImageUrl1 = _orderImageProcessor.ImageUrl1 ?? order.Image1;
                ImageUrl2 = _orderImageProcessor.ImageUrl2 ?? order.Image2;
                ImageUrl3 = _orderImageProcessor.ImageUrl3 ?? order.Image3;
                BoxedImage = order.BoxedImage;
                IsImageUploaded = true;

                // Cập nhật danh sách mặt hàng
                Items.Clear();
                if (order != null && order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        Items.Add(new OrderItem
                        {
                            CategoryId = item.CategoryId,
                            CategoryName = item.CategoryName,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            UnitPrice = item.UnitPrice,
                            Count = item.Count,
                            Total = item.Total
                        });
                    }
                }

                // Cập nhật tổng tiền
                Total = order.Total;
                OnPropertyChanged(nameof(Total));

                // Cập nhật đơn hàng hiện tại
                if (Order != null)
                {
                    Order.Items = new System.Collections.Generic.List<OrderItem>(Items);
                    Order.Total = Total;
                    Order.OrderId = order.OrderId;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi xử lý hình ảnh: {ex.Message}";
                throw;
            }
        }

        /// <summary>
        /// Xử lý thanh toán đơn hàng
        /// </summary>
        public async Task ProcessPayment(PaymentMethodWindow.PaymentMethod paymentMethod)
        {
            try
            {
                var response = await _paymentProcessor.ProcessPayment(Order, paymentMethod);
                if (response != null)
                {
                    if (response.Status == "SUCCESS")
                    {
                        StateManager.TransitionTo(new PaymentCompletedState());
                    }
                    else if (response.Status == "PENDING")
                    {
                        // Đối với thanh toán QR Code, có thể đang chờ xác nhận
                        // Có thể sử dụng WebSocket để nhận thông báo khi thanh toán hoàn tất
                    }
                    else if (response.Status != null)
                    {
                        DialogService.ShowInfoDialog("Thông báo", "Thanh toán không thành công.");
                        StateManager.TransitionTo(new OrderCreatedState());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi xử lý thanh toán: {ex.Message}";
                StateManager.TransitionTo(new OrderCreatedState());
            }
        }

        /// <summary>
        /// Kiểm tra đơn hàng
        /// </summary>
        public bool ValidateOrder()
        {
            string errorMessage = string.Empty;
            if (Items == null || Items.Count == 0)
            {
                errorMessage = "Vui lòng thêm mặt hàng vào đơn hàng.";
            }

            foreach (var item in Items)
            {
                if (string.IsNullOrEmpty(item.ProductId))
                {
                    errorMessage = "Vui lòng chọn sản phẩm cho tất cả các mặt hàng.";
                }
                if (item.Count <= 0)
                {
                    errorMessage = "Số lượng mặt hàng phải lớn hơn 0.";
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                DialogService.ShowInfoDialog("Thông báo", errorMessage);
                return false;
            }

            return true;
        }

        #endregion

        #region WebSocket

        /// <summary>
        /// Khởi tạo WebSocket
        /// </summary>
        private async void InitializeWebSocket()
        {
            try
            {
                _webSocketService.MessageReceived += OnWebSocketMessageReceived;
                await _webSocketService.ConnectAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Không thể kết nối WebSocket: {ex.Message}";
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhận message từ WebSocket
        /// </summary>
        private void OnWebSocketMessageReceived(object sender, string message)
        {
            // TODO: Xử lý message từ WebSocket ở đây
            Console.WriteLine($"Nhận message từ WebSocket: {message}");
        }

        #endregion

        /// <summary>
        /// Bắt đầu chế độ đọc thẻ tự động
        /// </summary>
        public async Task StartAutoCardReadingAsync()
        {
            if (!IsCardReaderActive)
            {
                await _cardReaderService.StartListeningAsync();
                IsCardReaderActive = true;
                CardReaderStatus = "Đang chờ quét thẻ...";
            }
        }

        /// <summary>
        /// Dừng chế độ đọc thẻ tự động
        /// </summary>
        public async Task StopAutoCardReadingAsync()
        {
            if (IsCardReaderActive)
            {
                await _cardReaderService.StopListeningAsync();
                IsCardReaderActive = false;
                CardReaderStatus = string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            // Hủy đăng ký các sự kiện camera
            CameraService.CameraInitialized -= OnCameraInitialized;
            CameraService.CameraError -= OnCameraError;
            CameraService.FrameUpdated -= OnCameraFrameUpdated;

            // Hủy đăng ký các sự kiện đọc thẻ
            _cardReaderService.CardScanned -= OnCardScanned;
            _cardReaderService.CardReadError -= OnCardReadError;

            // Dừng đọc thẻ
            if (IsCardReaderActive)
            {
                _cardReaderService.StopListeningAsync().Wait();
            }

            // Giải phóng tài nguyên camera
            CameraService.Dispose();
            _cardReaderService.Dispose();

            // Ngắt kết nối WebSocket
            //_webSocketService.MessageReceived -= OnWebSocketMessageReceived;
            //await _webSocketService.DisconnectAsync();
        }
    }
}
