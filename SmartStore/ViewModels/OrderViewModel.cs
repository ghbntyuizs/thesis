using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using SmartStorePOS.Services;
using SmartStorePOS.Views;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SmartStorePOS.ViewModels
{
    public class OrderViewModel : ViewModelBase, IDisposable
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IWebSocketService _webSocketService;

        private string _qrCodeUrl;
        private bool _isLoading;
        private bool _isCapturing;
        private string _errorMessage;
        private string _captureButtonText = "Bắt đầu chụp";
        private bool _hasImages = false;
        private bool _isCameraRunning = false;
        private string _overlayText;
        private string _orderText = "Xử lý đơn hàng";
        private bool _isShowOrderButton = true;
        private bool _isShowCancelButton = false;
        private bool _isReadOnly = true;
        private bool _isAllowAddNewRow = false;
        private string _imageUrl1;
        private string _imageUrl2;
        private string _imageUrl3;
        private bool _isImageUploaded = false;
        private const int _pageSize = int.MaxValue;
        private const int _pageNumber = 1;
        private bool _isLoadingProducts;

        private Order _order;
        private Order _lastOrder;
        private ObservableCollection<OrderItem> _items;
        private BitmapSource _capturedImage1;
        private BitmapSource _capturedImage2;
        private BitmapSource _capturedImage3;

        // tính năng cho phép chụp ảnh từ 3 camera
        private System.Windows.Threading.DispatcherTimer _timer;
        private VideoCapture[] _videoCaptureArray = new VideoCapture[3];
        private Mat[] _frameArray = new Mat[3];
        private bool[] _cameraInitialized = new bool[3];
        private ObservableCollection<ProductDTO> _productList;

        public ObservableCollection<ProductDTO> ProductList
        {
            get => _productList;
            set => SetProperty(ref _productList, value);
        }

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

        public decimal Total { get; private set; }

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

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                SetProperty(ref _isReadOnly, value);
            }
        }

        public bool IsAllowAddNewRow
        {
            get => _isAllowAddNewRow;
            set
            {
                SetProperty(ref _isAllowAddNewRow, value);
            }
        }

        public Order LastOrder
        {
            get => _lastOrder;
            set => SetProperty(ref _lastOrder, value);
        }

        public ICommand AddNewRowCommand { get; }
        public ICommand DeleteOrderItemCommand { get; }
        public ICommand NewOrderCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CaptureImagesCommand { get; }
        public ICommand ProcessOrderCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand CopyImageUrl1Command { get; }
        public ICommand CopyImageUrl2Command { get; }
        public ICommand CopyImageUrl3Command { get; }

        // Thêm vào constructor
        public OrderViewModel(IApiService apiService, INavigationService navigationService, IDialogService dialogService, IWebSocketService webSocketService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _webSocketService = webSocketService;

            //InitializeWebSocket();

            // Initialize collections
            Items = [];
            ProductList = [];

            // Initialize commands
            AddNewRowCommand = new RelayCommand(_ => AddNewRow());
            DeleteOrderItemCommand = new RelayCommand(item => DeleteOrderItem(item as OrderItem));
            NewOrderCommand = new RelayCommand(_ => _navigationService.NavigateTo<MainViewModel>());
            LogoutCommand = new RelayCommand(_ => _navigationService.NavigateTo<LoginViewModel>());
            CaptureImagesCommand = new RelayCommand(async _ => await CaptureImages(), _ => !_isCameraRunning || !IsLoading);
            ProcessOrderCommand = new RelayCommand(async _ => await ProcessOrder(), _ => _hasImages && !IsLoading);
            CancelCommand = new RelayCommand(_ => Cancel());
            HelpCommand = new RelayCommand(_ => _dialogService.ShowInfoDialog("Thông báo", "Tính năng đang được phát triển."));

            // Thêm các lệnh copy URL
            CopyImageUrl1Command = new RelayCommand(_ => CopyTextToClipboard(ImageUrl1));
            CopyImageUrl2Command = new RelayCommand(_ => CopyTextToClipboard(ImageUrl2));
            CopyImageUrl3Command = new RelayCommand(_ => CopyTextToClipboard(ImageUrl3));

            // Initialize order
            InitializeOrder();
        }

        /// <summary>
        /// Copy URL vào clipboard
        /// </summary>
        /// <param name="text"></param>
        private void CopyTextToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Clipboard.SetText(text);
                    _dialogService.ShowInfoDialog("Thông báo", "Đã copy URL vào clipboard!");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowInfoDialog("Lỗi", $"Không thể copy URL: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Khởi tạo đơn hàng mới
        /// </summary>
        private void InitializeOrder()
        {
            Order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                Status = "CREATED",
                CreatedDate = DateTime.Now,
                Items = [],
            };

            IsShowOrderButton = false;
        }

        /// <summary>
        /// Xử lý chụp ảnh từ camera
        /// </summary>
        /// <returns></returns>
        private async Task CaptureImages()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (!_isCameraRunning)
                {
                    // nếu mà có items thì cảnh báo
                    if (Items != null && Items.Count > 0)
                    {
                        var result = await _dialogService.ShowYesNoDialogAsync("Xác nhận", "Chụp ảnh mới sẽ xóa các mặt hàng hiện tại. Bạn có chắc chắn muốn tiếp tục?"); ;
                        if (result == false)
                            return;

                        Items.Clear();
                    }
                    // Initialize camera and start streaming
                    InitializeCamera();
                    CaptureButtonText = "Chụp";
                    OrderText = "Xử lý đơn hàng";
                    IsShowOrderButton = false;
                }
                else if (_isCapturing)
                {
                    if (IsLoading)
                        return;

                    // Currently streaming, capture the image
                    IsLoading = true;
                    OverlayText = "Đang chụp ảnh ...";
                    await Task.Delay(1000);
                    StopCameraStream();

                    CaptureButtonText = "Chụp lại";
                    OrderText = "Xử lý đơn hàng";
                    IsShowOrderButton = true;
                    _hasImages = true;
                    IsLoading = false;
                }
                else
                {
                    // Re-initialize camera for another capture
                    InitializeCamera();
                    CaptureButtonText = "Chụp";
                    IsShowOrderButton = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi chụp ảnh: {ex.Message}";
                StopCameraStream();
                CaptureButtonText = _hasImages ? "Chụp lại" : "Bắt đầu chụp";
            }
        }

        /// <summary>
        /// Khởi tạo camera và bắt đầu stream
        /// </summary>
        private void InitializeCamera()
        {
            try
            {
                // Dispose of any existing camera resources
                IsLoading = true;
                OverlayText = "Đang khởi tạo camera ...";
                StopCameraStream();

                // Create timer for camera feed
                _timer = new DispatcherTimer();
                _timer.Tick += (s, e) => UpdateCameraFrames();
                _timer.Interval = TimeSpan.FromMilliseconds(33); // Giá trị mặc định ban đầu ~30 FPS

                // Heavy operations can be done in background
                Task.Run(() =>
                {
                    try
                    {
                        // Đọc cấu hình camera ip
                        var cameraIp1 = ConfigurationManager.AppSettings["Camera1Ip"];
                        var cameraIp2 = ConfigurationManager.AppSettings["Camera2Ip"];
                        var cameraIp3 = ConfigurationManager.AppSettings["Camera3Ip"];

                        // Đọc cấu hình device id
                        var deviceId1 = ConfigurationManager.AppSettings["Camera1DeviceId"];
                        var deviceId2 = ConfigurationManager.AppSettings["Camera2DeviceId"];
                        var deviceId3 = ConfigurationManager.AppSettings["Camera3DeviceId"];

                        // Khởi tạo từng camera
                        InitializeSingleCamera(0, cameraIp1, deviceId1);
                        InitializeSingleCamera(1, cameraIp2, deviceId2);
                        InitializeSingleCamera(2, cameraIp3, deviceId3);

                        // Kiểm tra xem có camera nào được khởi tạo hay không
                        if (!_cameraInitialized[0] && !_cameraInitialized[1] && !_cameraInitialized[2])
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                StopCameraStream();
                                _dialogService.ShowInfoDialog("Lỗi", "Không thể khởi tạo bất kỳ camera nào, vui lòng kiểm tra cấu hình");
                                IsLoading = false;
                            });
                            return;
                        }

                        // Bắt đầu timer nếu có ít nhất một camera hoạt động
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _timer.Start();
                            IsCameraRunning = true;
                            _isCapturing = true;
                            IsLoading = false;
                            _isCameraRunning = true;
                            IsShowCancelButton = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            StopCameraStream();
                            _dialogService.ShowInfoDialog("Lỗi", $"Đã có lỗi xảy ra khi khởi chạy camera: {ex.Message}");
                            IsLoading = false;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khởi tạo camera: {ex.Message}";
                IsLoading = false;
            }
        }

        /// <summary>
        /// Khởi tạo một camera đơn
        /// </summary>
        private void InitializeSingleCamera(int index, string cameraIp, string deviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _videoCaptureArray[index] = new VideoCapture(Convert.ToInt32(deviceId));
                }
                else if (!string.IsNullOrEmpty(cameraIp))
                {
                    _videoCaptureArray[index] = new VideoCapture(cameraIp);
                }
                else
                {
                    return; // Không có thông tin camera, bỏ qua
                }

                _frameArray[index] = new Mat();

                if (_videoCaptureArray[index].IsOpened())
                {
                    _cameraInitialized[index] = true;
                }
                else
                {
                    _videoCaptureArray[index].Release();
                    _videoCaptureArray[index].Dispose();
                    _videoCaptureArray[index] = null;
                    _frameArray[index].Dispose();
                    _frameArray[index] = null;
                }
            }
            catch
            {
                if (_videoCaptureArray[index] != null)
                {
                    _videoCaptureArray[index].Release();
                    _videoCaptureArray[index].Dispose();
                    _videoCaptureArray[index] = null;
                }

                if (_frameArray[index] != null)
                {
                    _frameArray[index].Dispose();
                    _frameArray[index] = null;
                }
            }
        }

        /// <summary>
        /// Cập nhật khung hình từ tất cả camera
        /// </summary>
        private void UpdateCameraFrames()
        {
            // Xử lý camera 1
            if (_videoCaptureArray[0] != null && _videoCaptureArray[0].IsOpened() && _frameArray[0] != null)
            {
                try
                {
                    _videoCaptureArray[0].Read(_frameArray[0]);
                    if (!_frameArray[0].Empty())
                    {
                        CapturedImage1 = _frameArray[0].ToBitmapSource();
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi cho camera 1
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowInfoDialog("Lỗi Camera 1", $"{ex.Message}");
                    });
                }
            }

            // Xử lý camera 2
            if (_videoCaptureArray[1] != null && _videoCaptureArray[1].IsOpened() && _frameArray[1] != null)
            {
                try
                {
                    _videoCaptureArray[1].Read(_frameArray[1]);
                    if (!_frameArray[1].Empty())
                    {
                        CapturedImage2 = _frameArray[1].ToBitmapSource();
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi cho camera 2
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowInfoDialog("Lỗi Camera 2", $"{ex.Message}");
                    });
                }
            }

            // Xử lý camera 3
            if (_videoCaptureArray[2] != null && _videoCaptureArray[2].IsOpened() && _frameArray[2] != null)
            {
                try
                {
                    _videoCaptureArray[2].Read(_frameArray[2]);
                    if (!_frameArray[2].Empty())
                    {
                        CapturedImage3 = _frameArray[2].ToBitmapSource();
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi cho camera 3
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowInfoDialog("Lỗi Camera 3", $"{ex.Message}");
                    });
                }
            }
        }

        /// <summary>
        /// Dừng stream từ tất cả camera
        /// </summary>
        private void StopCameraStream()
        {
            // Stop the timer
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            // Release all camera resources
            for (int i = 0; i < 3; i++)
            {
                if (_videoCaptureArray[i] != null)
                {
                    _videoCaptureArray[i].Release();
                    _videoCaptureArray[i].Dispose();
                    _videoCaptureArray[i] = null;
                }

                if (_frameArray[i] != null)
                {
                    _frameArray[i].Dispose();
                    _frameArray[i] = null;
                }

                _cameraInitialized[i] = false;
            }

            IsCameraRunning = false;
            _isCapturing = false;
        }

        /// <summary>
        /// Xử lý gọi API để tạo đơn hàng từ ảnh
        /// </summary>
        private async Task HandleLoadOrderByImage()
        {
            // Lấy ra hình ảnh được chụp từ camera
            var image1 = CapturedImage1;
            var image2 = CapturedImage2;
            var image3 = CapturedImage3;

            // Reset URL trước khi upload
            ImageUrl1 = string.Empty;
            ImageUrl2 = string.Empty;
            ImageUrl3 = string.Empty;
            IsImageUploaded = false;

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
                    ImageUrl1 = uploadResponse1.image_url;
                }

                if (image2Path != null)
                {
                    var uploadResponse2 = await _apiService.UploadImageAsync(image2Path);
                    ImageUrl2 = uploadResponse2.image_url;
                }

                if (image3Path != null)
                {
                    var uploadResponse3 = await _apiService.UploadImageAsync(image3Path);
                    ImageUrl3 = uploadResponse3.image_url;
                }
            }
            else
            {
                // Dev mode
                ImageUrl1 = ConfigurationManager.AppSettings["Img1Url"];
                ImageUrl2 = ConfigurationManager.AppSettings["Img2Url"];
                ImageUrl3 = ConfigurationManager.AppSettings["Img3Url"];
            }

            IsImageUploaded = true;

            var orders = await _apiService.CreateOrderAsync(new CreateOrderRequest
            {
                Image1 = ImageUrl1,
                Image2 = ImageUrl2,
                Image3 = ImageUrl3,
            });

            LastOrder = orders;

            // Phần code còn lại giữ nguyên...
            Items.Clear();
            if (orders != null && orders.Items != null)
            {
                foreach (var item in orders.Items)
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

            // Calculate total
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Total;
            }

            if (total != orders.Total)
            {
                // TODO:
            }

            // Update order
            Total = orders.Total;
            OnPropertyChanged(nameof(Total));

            if (Order != null)
            {
                Order.Items = new System.Collections.Generic.List<OrderItem>(Items);
                Order.Total = Total;
                Order.OrderId = orders.OrderId;
            }
        }

        /// <summary>
        /// Xử lý đơn hàng
        /// </summary>
        private async Task ProcessOrder()
        {
            if (!_hasImages)
            {
                ErrorMessage = "Vui lòng chụp ảnh trước khi xử lý đơn hàng";
                return;
            }

            bool isCreateOrder = Items.Count == 0;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                if (isCreateOrder)
                {
                    OverlayText = "Đang xử lý đơn hàng ...";
                    await Task.Delay(2000);
                    await HandleLoadOrderByImage();
                    CaptureButtonText = "Chụp lại";
                    OrderText = "Thanh toán";
                    IsShowOrderButton = true;
                    IsReadOnly = false;
                    IsAllowAddNewRow = true;
                }
                else
                {
                    if (!ValidateOrder())
                        return;

                    var hasChanged = CheckOrderChanged();
                    if (hasChanged)
                    {
                        var dialogResult = _dialogService.ShowYesNoDialog("Xác nhận", "Đơn hàng đã thay đổi. Bạn có muốn tiếp tục không?");
                        if (dialogResult == false)
                            return;

                        var updateOrder = await _apiService.UpdateOrderWrongAsync(new UpdateOrderWrongRequest
                        {
                            OldOrderId = _lastOrder.OrderId,
                            Items = [.. Items.Select(x => new UpdateOrderWrongItems
                            {
                                ProductId = x.ProductId,
                                Count = x.Count,
                            })]
                        });

                        LastOrder = updateOrder;
                        Items.Clear();
                        if (updateOrder != null && updateOrder.Items != null)
                        {
                            foreach (var item in updateOrder.Items)
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

                        IsReadOnly = true;
                        IsAllowAddNewRow = false;
                    }

                    OverlayText = "Đang thực hiện thanh toán ...";
                    var paymentWindow = new PaymentMethodWindow();
                    if (paymentWindow.ShowDialog() == true)
                    {
                        switch (paymentWindow.SelectedMethod)
                        {
                            case PaymentMethodWindow.PaymentMethod.QRCode:
                                new QRCodeWindow($"aistore://payment/{Order.OrderId}").ShowDialog();
                                break;
                            case PaymentMethodWindow.PaymentMethod.MembershipCard:
                                var membershipWindow = _dialogService.GetService<MembershipCardWindow>();
                                MembershipCardViewModel membershipViewModel = membershipWindow.DataContext as MembershipCardViewModel;
                                membershipViewModel.Order = LastOrder;
                                membershipWindow.Owner = Application.Current.MainWindow;

                                // sự kiện khi đóng cửa sổ
                                membershipWindow.Closed += (s, e) =>
                                {
                                    if (membershipViewModel.PaymentResponse != null)
                                    {
                                        _dialogService.ShowInfoDialog("Thông báo", $"Đã thanh toán thành công số tiền {membershipViewModel.PaymentResponse.Amount:N0} VNĐ");
                                        InitFormState();
                                    }
                                    membershipViewModel.Dispose();
                                };
                                membershipWindow.ShowDialog();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi xử lý đơn hàng: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        private void Cancel()
        {
            // nếu mà đang có items thì cảnh báo
            if (Items != null && Items.Count > 0)
            {
                var result = _dialogService.ShowYesNoDialog("Xác nhận", "Hủy đơn hàng sẽ xóa tất cả các mặt hàng. Bạn có chắc chắn muốn tiếp tục?");
                if (result == false)
                    return;
                Items.Clear();
            }
            // Stop camera before navigating away
            StopCameraStream();

            // Reset the form state
            InitFormState();

            // Navigate back or reset the form
            //_navigationService.NavigateTo<MainViewModel>();
        }

        public void InitFormState()
        {
            // Reset all properties to their initial state
            IsReadOnly = true;
            IsAllowAddNewRow = false;
            IsLoading = false;
            IsCameraRunning = false;
            OverlayText = string.Empty;
            CaptureButtonText = "Bắt đầu chụp";
            OrderText = "Xử lý đơn hàng";
            IsShowOrderButton = true;
            IsShowCancelButton = false;
            _hasImages = false;

            ImageUrl1 = string.Empty;
            ImageUrl2 = string.Empty;
            ImageUrl3 = string.Empty;

            CapturedImage1 = null;
            CapturedImage2 = null;
            CapturedImage3 = null;

            IsImageUploaded = false;
            _isCapturing = false;
            _isCameraRunning = false;
            _isImageUploaded = false;
            _errorMessage = string.Empty;
            Items.Clear();
            LastOrder = null;
            Order = null;
            Total = 0;
            OnPropertyChanged(nameof(Total));
            InitializeOrder();
        }

        public void OnProductSelected(ProductDTO selectedProduct, OrderItem orderItem)
        {

            if (selectedProduct != null && orderItem != null)
            {
                orderItem.ProductId = selectedProduct.ProductId?.ToString();
                orderItem.ProductName = selectedProduct.ProductName;
                orderItem.CategoryId = selectedProduct.CategoryId?.ToString();
                orderItem.CategoryName = selectedProduct.CategoryName;
                orderItem.UnitPrice = (decimal)(selectedProduct.BasePrice ?? 0);
                orderItem.Count = 1;
                orderItem.Total = orderItem.UnitPrice * orderItem.Count;
                UpdateTotal();
            }
        }

        /// <summary>
        /// Validate Order
        /// </summary>
        private bool ValidateOrder()
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
                _dialogService.ShowInfoDialog("Thông báo", errorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hàm so sánh Items hiện tại và Items trong _lastOrder
        /// </summary>
        private bool CheckOrderChanged()
        {
            if (_lastOrder == null || _lastOrder.Items == null || Items == null)
                return false;

            // nếu số lượng mặt hàng không giống nhau thì chắc chắn là khác
            if (_lastOrder.Items.Count != Items.Count)
                return true;

            // So sánh từng mặt hàng
            for (int i = 0; i < _lastOrder.Items.Count; i++)
            {
                if (_lastOrder.Items[i].ProductId != Items[i].ProductId ||
                    _lastOrder.Items[i].Count != Items[i].Count ||
                    _lastOrder.Items[i].UnitPrice != Items[i].UnitPrice)
                {
                    return true;
                }
            }

            return false;
        }

        public void CalculateTotalPrice()
        {
            if (Items != null && Items.Count > 0)
            {
                Total = Items.Sum(x => x.Total);
                OnPropertyChanged(nameof(Total));
            }
        }

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

        private void OnWebSocketMessageReceived(object sender, string message)
        {
            // TODO: Xử lý message từ WebSocket ở đây
            Console.WriteLine($"Nhận message từ WebSocket: {message}");
        }

        public async Task LoadProducts(string searchText = null)
        {
            if (_isLoadingProducts) return;

            try
            {
                _isLoadingProducts = true;

                var request = new GetProductRequest
                {
                    pageNumber = _pageNumber,
                    pageSize = _pageSize,
                };

                var result = await _apiService.GetProductsAsync(request);
                if (result?.Items != null)
                {
                    foreach (var product in result.Items)
                    {
                        ProductList.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi tải danh sách sản phẩm: {ex.Message}";
            }
            finally
            {
                _isLoadingProducts = false;
            }
        }

        private void AddNewRow()
        {
            var newItem = new OrderItem
            {
                Count = 1
            };
            Items.Add(newItem);
        }

        private void DeleteOrderItem(OrderItem item)
        {
            if (item != null)
            {
                Items.Remove(item);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            Total = Items.Sum(x => x.Total);
            OnPropertyChanged(nameof(Total));

            if (Order != null)
            {
                Order.Items = new List<OrderItem>(Items);
                Order.Total = Total;
            }
        }

        /// <summary>
        /// Dispose camera resources
        /// </summary>
        public void Dispose()
        {
            //await _webSocketService.DisconnectAsync();
            //_webSocketService.MessageReceived -= OnWebSocketMessageReceived;

            StopCameraStream();
        }
    }
}
