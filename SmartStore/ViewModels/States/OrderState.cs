namespace SmartStorePOS.ViewModels.States
{
    /// <summary>
    /// Interface định nghĩa trạng thái đơn hàng
    /// </summary>
    public interface IOrderState
    {
        /// <summary>
        /// Khởi tạo trạng thái
        /// </summary>
        void Enter(OrderViewModel context);

        /// <summary>
        /// Xử lý khi người dùng ấn nút chụp ảnh
        /// </summary>
        Task HandleCaptureImages(OrderViewModel context);

        /// <summary>
        /// Xử lý khi người dùng ấn nút xử lý đơn hàng
        /// </summary>
        Task HandleProcessOrder(OrderViewModel context);

        /// <summary>
        /// Xử lý khi người dùng ấn nút hủy
        /// </summary>
        void HandleCancel(OrderViewModel context);

        /// <summary>
        /// Lấy tên trạng thái
        /// </summary>
        string GetStateName();
    }

    /// <summary>
    /// Trạng thái cơ sở chứa các hành vi chung
    /// </summary>
    public abstract class OrderStateBase : IOrderState
    {
        public abstract void Enter(OrderViewModel context);
        public abstract Task HandleCaptureImages(OrderViewModel context);
        public abstract Task HandleProcessOrder(OrderViewModel context);
        public abstract void HandleCancel(OrderViewModel context);
        public abstract string GetStateName();

        /// <summary>
        /// Reset toàn bộ trạng thái (bao gồm cả camera)
        /// </summary>
        protected void ResetOrderViewState(OrderViewModel context)
        {
            context.IsLoading = false;
            context.IsCaptured = false;
            context.IsCameraRunning = false;
            context.OverlayText = string.Empty;
            context.CaptureButtonText = "Bắt đầu chụp";
            context.OrderText = "Xử lý đơn hàng";
            context.IsShowOrderButton = true;
            context.IsShowCancelButton = false;
            context.ImageUrl1 = string.Empty;
            context.ImageUrl2 = string.Empty;
            context.ImageUrl3 = string.Empty;
            context.StreamImage1 = null;
            context.StreamImage2 = null;
            context.StreamImage3 = null;
            context.BoxedImage = null;
            context.IsImageUploaded = false;
            context.ErrorMessage = string.Empty;
            context.Items.Clear();
            context.Order = null;
            context.Total = 0;
            context.TotalBeforeDiscount = 0;
            context.TotalItemDiscount = 0;
            context.OrderDiscount = 0;
            context.InitializeOrder();
        }

        /// <summary>
        /// Reset state đơn hàng nhưng giữ nguyên camera
        /// </summary>
        protected void ResetOrderStateKeepCamera(OrderViewModel context)
        {
            context.IsLoading = false;
            context.IsCaptured = false;
            context.OverlayText = string.Empty;
            context.CaptureButtonText = "Chụp";
            context.OrderText = "Xử lý đơn hàng";
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = false;
            context.ImageUrl1 = string.Empty;
            context.ImageUrl2 = string.Empty;
            context.ImageUrl3 = string.Empty;
            context.CapturedImage1 = null;
            context.CapturedImage2 = null;
            context.CapturedImage3 = null;
            context.BoxedImage = null;
            context.IsImageUploaded = false;
            context.ErrorMessage = string.Empty;
            context.Items.Clear();
            context.Order = null;
            context.Total = 0;
            context.TotalBeforeDiscount = 0;
            context.TotalItemDiscount = 0;
            context.OrderDiscount = 0;
            context.InitializeOrder();
        }
    }
}
