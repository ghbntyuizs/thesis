using System.Configuration;

namespace SmartStorePOS.Models
{
    public class ProductDTO
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
        public decimal? BasePrice { get; set; }
        public string ImageUrl { get; set; }
        public bool IsSuspended { get; set; }
        public List<Inventory> Inventories { get; set; } = [];
        public void GetBasePriceByStoreId()
        {
            string storeid = ConfigurationManager.AppSettings["StoreId"];
            var inventory = Inventories.FirstOrDefault(i => i.ProductId == ProductId && string.Equals(storeid, i.StoreId, StringComparison.OrdinalIgnoreCase));
            if (inventory != null)
            {
                BasePrice = inventory.Price;
            }
        }
    }

    public class Inventory
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreCode { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
}
