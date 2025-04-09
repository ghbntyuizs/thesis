namespace SmartStorePOS.Models
{
    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Count { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
