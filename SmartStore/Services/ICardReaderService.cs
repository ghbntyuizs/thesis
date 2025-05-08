using System;
using System.Threading.Tasks;

namespace SmartStorePOS.Services
{
    /// <summary>
    /// Interface định nghĩa dịch vụ đọc thẻ tự động
    /// </summary>
    public interface ICardReaderService : IDisposable
    {
        /// <summary>
        /// Sự kiện khi thẻ được quét thành công
        /// </summary>
        event EventHandler<string> CardScanned;

        /// <summary>
        /// Sự kiện khi có lỗi đọc thẻ
        /// </summary>
        event EventHandler<Exception> CardReadError;

        /// <summary>
        /// Bắt đầu lắng nghe thẻ
        /// </summary>
        Task StartListeningAsync();

        /// <summary>
        /// Dừng lắng nghe thẻ
        /// </summary>
        Task StopListeningAsync();

        /// <summary>
        /// Trạng thái đang lắng nghe
        /// </summary>
        bool IsListening { get; }
    }
}
