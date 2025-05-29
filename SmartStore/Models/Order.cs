namespace SmartStorePOS.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string OrderCode { get; set; }
        public string Customer { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
        public decimal TotalBeforeDiscount { get; set; }
        public decimal TotalItemDiscount { get; set; }
        public decimal OrderDiscount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string BoxedImage { get; set; }
    }
}
