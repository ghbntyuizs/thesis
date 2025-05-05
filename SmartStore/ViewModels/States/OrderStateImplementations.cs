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
            // Không có gì để hủy ở trạng thái ban đầu
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
            context.CameraService.StopCameraStream();
            context.StateManager.TransitionTo(new InitialState());
        }

        public override string GetStateName() => "CameraInitializing";
    }

    /// <summary>
    /// Trạng thái camera đang hoạt động
    /// </summary>
    public class CameraActiveState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsLoading = false;
            context.IsCameraRunning = true;
            context.CaptureButtonText = "Chụp";
            context.IsShowOrderButton = false;
            context.IsShowCancelButton = true;
        }

        public override async Task HandleCaptureImages(OrderViewModel context)
        {
            context.IsLoading = true;
            context.OverlayText = "Đang chụp ảnh ...";
            await Task.Delay(1000);

            context.CameraService.StopCameraStream();
            context.StateManager.TransitionTo(new ImageCapturedState());
        }

        public override Task HandleProcessOrder(OrderViewModel context)
        {
            // Camera đang hoạt động, không xử lý đơn hàng
            return Task.CompletedTask;
        }

        public override void HandleCancel(OrderViewModel context)
        {
            context.CameraService.StopCameraStream();
            context.StateManager.TransitionTo(new InitialState());
        }

        public override string GetStateName() => "CameraActive";
    }

    /// <summary>
    /// Trạng thái đã chụp ảnh
    /// </summary>
    public class ImageCapturedState : OrderStateBase
    {
        public override void Enter(OrderViewModel context)
        {
            context.IsLoading = false;
            context.IsCameraRunning = false;
            context.CaptureButtonText = "Chụp lại";
            context.OrderText = "Xử lý đơn hàng";
            context.IsShowOrderButton = true;
            context.IsShowCancelButton = true;
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

            context.StateManager.TransitionTo(new CameraInitializingState());
            await context.CameraService.InitializeCamera();
        }

        public override async Task HandleProcessOrder(OrderViewModel context)
        {
            context.IsLoading = true;
            context.ErrorMessage = string.Empty;
            context.OverlayText = "Đang xử lý đơn hàng ...";

            await Task.Delay(2000);
            await context.HandleLoadOrderByImage();

            context.StateManager.TransitionTo(new OrderCreatedState());
        }

        public override void HandleCancel(OrderViewModel context)
        {
            context.CameraService.StopCameraStream();
            context.StateManager.TransitionTo(new InitialState());
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
            context.IsShowCancelButton = true;
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

            context.StateManager.TransitionTo(new CameraInitializingState());
            await context.CameraService.InitializeCamera();
        }

        public override async Task HandleProcessOrder(OrderViewModel context)
        {
            if (!context.ValidateOrder())
                return;

            var hasChanged = context.CheckOrderChanged();
            if (hasChanged)
            {
                var dialogResult = context.DialogService.ShowYesNoDialog("Xác nhận", "Đơn hàng đã thay đổi. Bạn có muốn tiếp tục không?");
                if (dialogResult == false)
                    return;

                await context.UpdateOrder();
            }

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

            context.CameraService.StopCameraStream();
            context.StateManager.TransitionTo(new InitialState());
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
            ResetOrderViewState(context);
            context.StateManager.TransitionTo(new InitialState());
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
