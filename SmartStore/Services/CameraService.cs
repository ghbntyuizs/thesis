using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Configuration;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Triển khai ICameraService
    /// </summary>
    public class CameraService(IDialogService dialogService) : ICameraService, IDisposable
    {
        private readonly IDialogService _dialogService = dialogService;

        // Trạng thái camera
        private bool _isCameraRunning = false;

        // Timer để cập nhật frame từ camera
        private DispatcherTimer _timer;

        // Mảng VideoCapture cho 3 camera
        private VideoCapture[] _videoCaptureArray = new VideoCapture[3];

        // Mảng frame cho 3 camera
        private Mat[] _frameArray = new Mat[3];

        // Trạng thái khởi tạo của từng camera
        private bool[] _cameraInitialized = new bool[3];

        // Hình ảnh từ 3 camera
        private BitmapSource _image1;
        private BitmapSource _image2;
        private BitmapSource _image3;

        // Sự kiện
        public event EventHandler CameraInitialized;
        public event EventHandler<Exception> CameraError;
        public event EventHandler FrameUpdated;

        // Properties
        public bool IsCameraRunning => _isCameraRunning;
        public BitmapSource Image1 { get => _image1; private set => _image1 = value; }
        public BitmapSource Image2 { get => _image2; private set => _image2 = value; }
        public BitmapSource Image3 { get => _image3; private set => _image3 = value; }

        /// <summary>
        /// Khởi tạo camera và bắt đầu stream
        /// </summary>
        public async Task InitializeCamera()
        {
            try
            {
                // Giải phóng tài nguyên camera hiện có
                StopCameraStream();

                // Tạo timer cho camera feed
                _timer = new DispatcherTimer();
                _timer.Tick += (s, e) => UpdateCameraFrames();
                _timer.Interval = TimeSpan.FromMilliseconds(33); // Giá trị mặc định ban đầu ~30 FPS

                // Thực hiện các thao tác nặng trong background
                await Task.Run(async () =>
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
                        await RunCameraInitialization(cameraIp1, cameraIp2, cameraIp3, deviceId1, deviceId2, deviceId3);

                        // Kiểm tra xem có camera nào được khởi tạo hay không
                        if (!_cameraInitialized[0] && !_cameraInitialized[1] && !_cameraInitialized[2])
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                StopCameraStream();
                                _dialogService.ShowInfoDialog("Lỗi", "Không thể khởi tạo bất kỳ camera nào, vui lòng kiểm tra cấu hình");
                                CameraError?.Invoke(this, new Exception("Không thể khởi tạo bất kỳ camera nào"));
                            });
                            return;
                        }

                        // Bắt đầu timer nếu có ít nhất một camera hoạt động
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _timer.Start();
                            _isCameraRunning = true;
                            CameraInitialized?.Invoke(this, EventArgs.Empty);
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StopCameraStream();
                            _dialogService.ShowInfoDialog("Lỗi", $"Đã có lỗi xảy ra khi khởi chạy camera: {ex.Message}");
                            CameraError?.Invoke(this, ex);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                CameraError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Chạy khởi tạo camera song song
        /// </summary>
        public async Task RunCameraInitialization(string cameraIp1, string cameraIp2, string cameraIp3,
                                               string deviceId1, string deviceId2, string deviceId3)
        {
            var cameraInitTasks = new Task[]
            {
                Task.Run(() => InitializeSingleCamera(0, cameraIp1, deviceId1)),
                Task.Run(() => InitializeSingleCamera(1, cameraIp2, deviceId2)),
                Task.Run(() => InitializeSingleCamera(2, cameraIp3, deviceId3))
            };

            // Đợi tất cả các khởi tạo camera hoàn thành
            await Task.WhenAll(cameraInitTasks);
        }

        /// <summary>
        /// Khởi tạo một camera đơn
        /// </summary>
        public void InitializeSingleCamera(int index, string cameraIp, string deviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _videoCaptureArray[index] = new VideoCapture(Convert.ToInt32(deviceId), VideoCaptureAPIs.DSHOW);
                }
                else if (!string.IsNullOrEmpty(cameraIp))
                {
                    _videoCaptureArray[index] = new VideoCapture(cameraIp);
                }
                else
                {
                    return; // Không có thông tin camera, bỏ qua
                }

                // Kiểm tra xem camera đã mở thành công hay chưa
                if (_videoCaptureArray[index].IsOpened())
                {
                    var capture = _videoCaptureArray[index];
                    capture.Set(VideoCaptureProperties.FrameWidth, 1920);
                    capture.Set(VideoCaptureProperties.FrameHeight, 1080);

                    // Khởi tạo frame cho camera
                    _frameArray[index] = new Mat();

                    _cameraInitialized[index] = true;
                }
                else
                {
                    ReleaseCameraResources(index);
                }
            }
            catch
            {
                ReleaseCameraResources(index);
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên camera
        /// </summary>
        private void ReleaseCameraResources(int index)
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

        /// <summary>
        /// Cập nhật frame từ tất cả camera
        /// </summary>
        private void UpdateCameraFrames()
        {
            UpdateSingleCameraFrame(0, ref _image1);
            UpdateSingleCameraFrame(1, ref _image2);
            UpdateSingleCameraFrame(2, ref _image3);

            FrameUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Cập nhật frame từ một camera
        /// </summary>
        private void UpdateSingleCameraFrame(int index, ref BitmapSource imageSource)
        {
            if (_videoCaptureArray[index] != null && _videoCaptureArray[index].IsOpened() && _frameArray[index] != null)
            {
                try
                {
                    _videoCaptureArray[index].Read(_frameArray[index]);
                    if (!_frameArray[index].Empty())
                    {
                        imageSource = _frameArray[index].ToBitmapSource();
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi cho camera
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowInfoDialog($"Lỗi Camera {index + 1}", $"{ex.Message}");
                        CameraError?.Invoke(this, ex);
                    });
                }
            }
        }

        /// <summary>
        /// Dừng stream từ tất cả camera
        /// </summary>
        public void StopCameraStream()
        {
            // Dừng timer
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            // Giải phóng tất cả tài nguyên camera
            for (int i = 0; i < 3; i++)
            {
                ReleaseCameraResources(i);
                _cameraInitialized[i] = false;
            }

            _isCameraRunning = false;
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            StopCameraStream();
        }
    }
}
