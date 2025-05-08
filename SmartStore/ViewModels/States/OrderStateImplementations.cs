using SmartStorePOS.Views;

namespace SmartStorePOS.ViewModels.States
{
    /// <summary>
    /// Trạng thái khởi tạo - chưa có đơn hàng
    /// </summary>
    public class InitialState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = false;
            context.CaptureButtonText = "Bắt đầu chụp";
            context.OrderText = "Xử lý đơn hàng";

        }

        public override async Task HandleCaptureImages(OrderViewModel context)
        {
            context.ErrorMessage = string.Empty;
            context.IsLoading = true;
            context.OverlayText = "Đang khởi tạo camera ...";

            context.StateManager.TransitionTo(new CameraInitializingState());
            await context.CameraService.InitializeCamera();
        }

        public override Task HandleProcessOrder(OrderViewModel context)
        {
            context.ErrorMessage = "Vui lòng chụp ảnh trước khi xử lý đơn hàng";
            return Task.CompletedTask;
        }

        public override void HandleCancel(OrderViewModel context)
        {
            // Không tắt camera, chỉ chuyển về trạng thái CameraActive
            ResetOrderStateKeepCamera(context);
        }

        public override string GetStateName() => "Initial";
    }

    /// <summary>
    /// Trạng thái đang khởi tạo camera
    /// </summary>
    public class CameraInitializingState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsLoading = true;
            context.OverlayText = "Đang khởi tạo camera ...";
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = true;
        }

        public override Task HandleCaptureImages(OrderViewModel context)
        {
            // Đang khởi tạo camera, không làm gì
            return Task.CompletedTask;
        }

        public override Task HandleProcessOrder(OrderViewModel context)
        {
            // Đang khởi tạo camera, không làm gì
            return Task.CompletedTask;
        }

        public override void HandleCancel(OrderViewModel context)
        {
            // Không tắt camera, chỉ chuyển về trạng thái CameraActive
            ResetOrderStateKeepCamera(context);
            context.StateManager.TransitionTo(new CameraActiveState());
        }

        public override string GetStateName() => "CameraInitializing";
    }

    /// <summary>
    /// Trạng thái camera đang hoạt động
    /// </summary>
    public class CameraActiveState : OrderStateBase
    {
        public override async void Enter(OrderViewModel context)
        {
            ResetOrderStateKeepCamera(context);
            context.IsLoading = false;
            context.IsCameraRunning = true;
            context.CaptureButtonText = "Chụp";
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = false;

            // Đảm bảo camera đang stream khi vào trạng thái này
            if (!context.CameraService.IsCameraRunning)
            {
                context.IsLoading = true;
                context.OverlayText = "Đang khởi tạo camera ...";
                await context.CameraService.InitializeCamera();
                context.IsLoading = false;
            }
        }

        public override async Task HandleCaptureImages(OrderViewModel context)
        {
            context.IsLoading = true;
            context.OverlayText = "Đang chụp ảnh ...";
            context.IsCaptured = true;

            // Lưu trữ hình ảnh hiện tại mà không dừng camera
            context.CapturedImage1 = context.CameraService.Image1;
            context.CapturedImage2 = context.CameraService.Image2;
            context.CapturedImage3 = context.CameraService.Image3;

            await Task.Delay(500);
            context.IsLoading = false;

            // Không dừng camera stream, vẫn giữ camera hoạt động
            context.StateManager.TransitionTo(new ImageCapturedState());
        }

        public override Task HandleProcessOrder(OrderViewModel context)
        {
            // Camera đang hoạt động, không xử lý đơn hàng
            return Task.CompletedTask;
        }

        public override void HandleCancel(OrderViewModel context)
        {
            // Giữ nguyên camera, chỉ chuyển về trạng thái CameraActive và reset state
            ResetOrderStateKeepCamera(context);
            context.StateManager.TransitionTo(new CameraActiveState());
        }

        public override string GetStateName() => "CameraActive";
    }

    /// <summary>
    /// Trạng thái đã chụp ảnh
    /// </summary>
    public class ImageCapturedState : OrderStateBase
    {
        public override async void Enter(OrderViewModel context)
        {
            context.IsLoading = false;
            // Không đặt IsCameraRunning = false vì camera vẫn đang chạy
            context.CaptureButtonText = "Chụp lại";
            context.OrderText = "Xử lý đơn hàng";
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = true;
            await HandleProcessOrder(context);
        }

        public override async Task HandleCaptureImages(OrderViewModel context)
        {
            // Xóa đơn hàng hiện tại nếu có
            if (context.Items.Count > 0)
            {
                var result = await context.DialogService.ShowYesNoDialogAsync("Xác nhận", "Chụp ảnh mới sẽ xóa các mặt hàng hiện tại. Bạn có chắc chắn muốn tiếp tục?");
                if (result == false)
                    return;

                context.Items.Clear();
            }

            // Reset các state liên quan đến đơn hàng và BoxedImage
            context.BoxedImage = null;
            context.IsImageUploaded = false;
            context.Order = null;
            context.Total = 0;
            context.InitializeOrder();

            // Nếu camera đang hoạt động, chỉ cần chuyển về trạng thái CameraActive
            if (context.CameraService.IsCameraRunning)
            {
                context.StateManager.TransitionTo(new CameraActiveState());
            }
            else
            {
                // Nếu camera chưa khởi tạo, cần khởi tạo lại
                context.StateManager.TransitionTo(new CameraInitializingState());
                await context.CameraService.InitializeCamera();
            }
        }

        public override async Task HandleProcessOrder(OrderViewModel context)
        {
            context.IsLoading = true;
            context.ErrorMessage = string.Empty;
            context.OverlayText = "Đang xử lý đơn hàng ...";
            context.IsShowCancelButton = false;

            await Task.Delay(1000);
            await context.HandleLoadOrderByImage();

            context.StateManager.TransitionTo(new OrderCreatedState());
        }

        public override void HandleCancel(OrderViewModel context)
        {
            // Giữ nguyên camera, reset các state và về trạng thái CameraActive
            ResetOrderStateKeepCamera(context);
            context.StateManager.TransitionTo(new CameraActiveState());
        }

        public override string GetStateName() => "ImageCaptured";
    }

    /// <summary>
    /// Trạng thái đơn hàng đã được tạo
    /// </summary>
    public class OrderCreatedState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsLoading = false;
            context.CaptureButtonText = "Chụp lại";
            context.OrderText = "Thanh toán";
            context.IsShowOrderButton = true;
            if (!string.IsNullOrEmpty(context.LastPostOrder.Warning))
            {
                context.DialogService.ShowInfoDialog("Cảnh báo", "Tổng số lượng sản phẩm không khớp so với số lượng trên hình");
            }
            else if (context.Items.Count == 0)
            {
                context.IsShowOrderButton = false;
                context.DialogService.ShowInfoDialog("Cảnh báo", "Không tìm thấy mặt hàng nào trong ảnh. Vui lòng chụp lại.");
                context.StateManager.TransitionTo(new CameraActiveState());
            }
            else
            {
                context.IsShowCancelButton = true;
            }
        }

        public override async Task HandleCaptureImages(OrderViewModel context)
        {
            if (context.Items.Count > 0)
            {
                var result = await context.DialogService.ShowYesNoDialogAsync("Xác nhận", "Chụp ảnh mới sẽ xóa các mặt hàng hiện tại. Bạn có chắc chắn muốn tiếp tục?");
                if (result == false)
                    return;

                context.Items.Clear();
            }

            // Reset các state liên quan đến đơn hàng và BoxedImage
            context.BoxedImage = null;
            context.IsImageUploaded = false;
            context.Order = null;
            context.Total = 0;
            context.InitializeOrder();

            // Nếu camera đang hoạt động, chỉ cần chuyển về trạng thái CameraActive
            if (context.CameraService.IsCameraRunning)
            {
                context.StateManager.TransitionTo(new CameraActiveState());
            }
            else
            {
                // Nếu camera chưa khởi tạo, cần khởi tạo lại
                context.StateManager.TransitionTo(new CameraInitializingState());
                await context.CameraService.InitializeCamera();
            }
        }

        public override async Task HandleProcessOrder(OrderViewModel context)
        {
            if (!context.ValidateOrder())
                return;

            context.OverlayText = "Đang thực hiện thanh toán ...";
            context.StateManager.TransitionTo(new PaymentProcessingState());

            var paymentWindow = new PaymentMethodWindow();
            if (paymentWindow.ShowDialog() == true)
            {
                await context.ProcessPayment(paymentWindow.SelectedMethod);
            }
            else
            {
                context.StateManager.TransitionTo(new OrderCreatedState());
            }
        }

        public override void HandleCancel(OrderViewModel context)
        {
            if (context.Items.Count > 0)
            {
                var result = context.DialogService.ShowYesNoDialog("Xác nhận", "Hủy đơn hàng sẽ xóa tất cả các mặt hàng. Bạn có chắc chắn muốn tiếp tục?");
                if (result == false)
                    return;
            }

            // Giữ nguyên camera, chỉ reset các state và chuyển về CameraActive
            ResetOrderStateKeepCamera(context);
            context.StateManager.TransitionTo(new CameraActiveState());
        }

        public override string GetStateName() => "OrderCreated";
    }

    /// <summary>
    /// Trạng thái đang xử lý thanh toán
    /// </summary>
    public class PaymentProcessingState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsLoading = true;
            context.OverlayText = "Đang thực hiện thanh toán ...";
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = false;
        }

        public override Task HandleCaptureImages(OrderViewModel context)
        {
            // Đang xử lý thanh toán, không làm gì
            return Task.CompletedTask;
        }

        public override Task HandleProcessOrder(OrderViewModel context)
        {
            // Đang xử lý thanh toán, không làm gì
            return Task.CompletedTask;
        }

        public override void HandleCancel(OrderViewModel context)
        {
            // Đang xử lý thanh toán, không thể hủy
        }

        public override string GetStateName() => "PaymentProcessing";
    }

    /// <summary>
    /// Trạng thái thanh toán hoàn tất
    /// </summary>
    public class PaymentCompletedState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsLoading = false;
            context.DialogService.ShowInfoDialog("Thông báo", $"Đã thanh toán thành công số tiền {context.Total:N0} VNĐ");

            // Reset lại trạng thái về ban đầu
            //ResetOrderViewState(context);
            ResetOrderStateKeepCamera(context);
            //context.StateManager.TransitionTo(new InitialState());
            context.StateManager.TransitionTo(new CameraActiveState());
        }

        public override Task HandleCaptureImages(OrderViewModel context)
        {
            return Task.CompletedTask;
        }

        public override Task HandleProcessOrder(OrderViewModel context)
        {
            return Task.CompletedTask;
        }

        public override void HandleCancel(OrderViewModel context)
        {
        }

        public override string GetStateName() => "PaymentCompleted";
    }
}
