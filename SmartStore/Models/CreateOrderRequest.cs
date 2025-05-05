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
}
