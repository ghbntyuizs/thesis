using System.Windows.Media.Imaging;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Service xử lý các thao tác liên quan đến camera
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// Sự kiện khi camera được khởi tạo thành công
        /// </summary>
        event EventHandler CameraInitialized;

        /// <summary>
        /// Sự kiện khi xảy ra lỗi camera
        /// </summary>
        event EventHandler<Exception> CameraError;

        /// <summary>
        /// Sự kiện khi frame được cập nhật
        /// </summary>
        event EventHandler FrameUpdated;

        /// <summary>
        /// Khởi tạo camera
        /// </summary>
        Task InitializeCamera();

        /// <summary>
        /// Dừng stream từ tất cả camera
        /// </summary>
        void StopCameraStream();

        /// <summary>
        /// Khởi tạo một camera đơn
        /// </summary>
        void InitializeSingleCamera(int index, string cameraIp, string deviceId);

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        void Dispose();

        /// <summary>
        /// Trạng thái camera đang chạy
        /// </summary>
        bool IsCameraRunning { get; }

        /// <summary>
        /// Hình ảnh từ camera 1
        /// </summary>
        BitmapSource Image1 { get; }

        /// <summary>
        /// Hình ảnh từ camera 2
        /// </summary>
        BitmapSource Image2 { get; }

        /// <summary>
        /// Hình ảnh từ camera 3
        /// </summary>
        BitmapSource Image3 { get; }
    }
}
