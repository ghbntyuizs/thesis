namespace SmartStorePOS.Models
{
    public class GetProductResponse
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItem { get; set; }
        public List<ProductDTO> Items { get; set; } = [];
    }
}
