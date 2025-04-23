using System.Configuration;

namespace SmartStorePOS.Models
{
    public class CreateOrderRequest
    {
        public string DeviceId { get; set; } = ConfigurationManager.AppSettings["DeviceId"];
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
    }

    public class UpdateOrderWrongRequest
    {
        public string StaffId { get; set; } = ConfigurationManager.AppSettings["StaffId"];
        public string OldOrderId { get; set; }
        public List<UpdateOrderWrongItems> Items { get; set; } = [];
    }

    public class UpdateOrderWrongItems
    {
        public string ProductId { get; set; }
        public int Count { get; set; }
    }
}
