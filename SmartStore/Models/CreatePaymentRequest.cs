namespace SmartStorePOS.Models
{
    public class CreatePaymentRequest
    {
        public Guid CardId { get; set; }
        public Guid OrderId { get; set; }
    }
}
