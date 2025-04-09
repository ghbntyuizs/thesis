using OpenCvSharp;
using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SmartStorePOS.ViewModels
{

    public class CameraSelectorViewModel : ViewModelBase
    {
        private ObservableCollection<CameraDevice> _cameraDevices;
        private CameraDevice _selectedCamera;
        private BitmapImage _previewImage;
        private bool _isPreviewAvailable;
        private bool _isPreviewActive;
        private VideoCapture _videoCapture;
        private Task _previewTask;
        private bool _isRunning;
        private string _ipUrl;

        public event PropertyChangedEventHandler PropertyChanged;

        public CameraSelectorViewModel()
        {
            CameraDevices = new ObservableCollection<CameraDevice>();
            Title = "Select Camera Device";
            InitializeCommands();
            RefreshCameraList();
        }

        #region Properties

        public string Title { get; set; }

        public ObservableCollection<CameraDevice> CameraDevices
        {
            get => _cameraDevices;
            set
            {
                _cameraDevices = value;
                OnPropertyChanged();
            }
        }

        public CameraDevice SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                OnPropertyChanged();
                UpdateCanSelectCamera();
                if (_isPreviewActive)
                {
                    StopPreview();
                    StartPreview();
                }
            }
        }

        public BitmapImage PreviewImage
        {
            get => _previewImage;
            set
            {
                _previewImage = value;
                OnPropertyChanged();
            }
        }

        public bool IsPreviewAvailable
        {
            get => _isPreviewAvailable;
            set
            {
                _isPreviewAvailable = value;
                OnPropertyChanged();
            }
        }

        public bool IsPreviewActive
        {
            get => _isPreviewActive;
            set
            {
                _isPreviewActive = value;
                OnPropertyChanged();
            }
        }

        public bool CanSelectCamera => SelectedCamera != null;

        public string IpUrl
        {
            get => _ipUrl;
            set
            {
                _ipUrl = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand RefreshCamerasCommand { get; private set; }
        public ICommand StartPreviewCommand { get; private set; }
        public ICommand StopPreviewCommand { get; private set; }
        public ICommand SelectCameraCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void InitializeCommands()
        {
            RefreshCamerasCommand = new RelayCommand(_ => RefreshCameraList());
            StartPreviewCommand = new RelayCommand(_ => StartPreview(), _ => (SelectedCamera != null && !_isPreviewActive) || !string.IsNullOrEmpty(IpUrl));
            StopPreviewCommand = new RelayCommand(_ => StopPreview(), _ => _isPreviewActive);
            SelectCameraCommand = new RelayCommand(_ => SelectCamera(), _ => CanSelectCamera);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        #endregion

        #region Methods

        private void RefreshCameraList()
        {
            CameraDevices.Clear();

            // Tìm camera cục bộ
            int maxDevicesToCheck = 10; // Giới hạn số lượng camera cần kiểm tra
            for (int i = 0; i < maxDevicesToCheck; i++)
            {
                using (var capture = new VideoCapture(i))
                {
                    if (capture.IsOpened())
                    {
                        var camera = new CameraDevice(
                            name: $"Local Camera {i}",
                            deviceId: i.ToString(),
                            devicePath: $"path/to/camera/{i}" // Có thể bỏ nếu không cần
                        );
                        CameraDevices.Add(camera);
                    }
                }
            }
        }

        private void StartPreview()
        {
            if (SelectedCamera == null || _isPreviewActive) return;

            _isRunning = true;
            IsPreviewActive = true;

            // Kiểm tra nếu là camera IP hay camera cục bộ
            if (!string.IsNullOrEmpty(SelectedCamera.IpUrl))
            {
                _videoCapture = new VideoCapture(SelectedCamera.IpUrl); // Sử dụng URL cho camera IP
            }
            else
            {
                _videoCapture = new VideoCapture(int.Parse(SelectedCamera.DeviceId)); // Sử dụng index cho camera cục bộ
            }

            if (!_videoCapture.IsOpened())
            {
                IsPreviewAvailable = false;
                return;
            }

            IsPreviewAvailable = true;

            _previewTask = Task.Run(() =>
            {
                using (var mat = new Mat())
                {
                    while (_isRunning && _videoCapture.IsOpened())
                    {
                        _videoCapture.Read(mat);
                        if (!mat.Empty())
                        {
                            var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
                            PreviewImage = BitmapToImageSource(bitmap);
                        }
                        Task.Delay(33).Wait(); // ~30 FPS
                    }
                }
            });
        }

        private void StopPreview()
        {
            try
            {
                _isRunning = false;
                IsPreviewActive = false;
                IsPreviewAvailable = false;
                PreviewImage = null;
                _videoCapture?.Release();
                _videoCapture?.Dispose();
                _previewTask?.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping preview: {ex.Message}");
            }
        }

        private void SelectCamera()
        {
            StopPreview();

            // nếu string khác null thì là camera IP
            if (!string.IsNullOrEmpty(IpUrl))
            {
                SecureStorageHelper.SaveConfigDynamic(nameof(AppConfig.CameraIp), IpUrl);
            }
            else
            {
                SecureStorageHelper.SaveConfigDynamic(nameof(AppConfig.DeviceId), SelectedCamera.DeviceId);
            }


            // Logic để chọn camera (có thể truyền SelectedCamera ra ngoài)
            System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .SingleOrDefault(x => x.DataContext == this)?.Close();
        }

        private void Cancel()
        {
            StopPreview();
            System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .SingleOrDefault(x => x.DataContext == this)?.Close();
        }

        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private void UpdateCanSelectCamera()
        {
            OnPropertyChanged(nameof(CanSelectCamera));
        }

        #endregion
    }
}
