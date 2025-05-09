namespace SmartStorePOS.Models
{
    public class PaymentResponse
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Status { get; set; }
        public decimal RemainBalance { get; set; }

        public string? Msg { get; set; }
    }
}
