Tôi cần sửa lại một chút ứng dụng này. 
- Khi mở ứng dụng mở luôn vào màn hình tạo đơn hàng mới thay vì màn hình chọn thao tác
- Tại màn hình tạo đơn hàng mới.
    + Chỉ cần hiển thị 1 camera thay vì 3 camera như hiện tại (logic xử lý vẫn giữ nguyên như hiện tại, chỉ là giao diện hiển thị 1 camera).
    + Đưa nút trợ giúp vào trong màn hình tạo đơn hàng luôn
- Khi thực hiện thanh toán, bổ sung thêm màn hình chọn hình thức thanh toán là QRCode và Thẻ ngân hàng.
    + Nếu chọn QRCode thì tạo new QRCodeWindow($"aistore://payment/{Order.OrderId}").ShowDialog(); như cũ
    + Nếu chọn Thẻ ngân hàng thì tạo new MembershipCardWindow().ShowDialog(); như cũ