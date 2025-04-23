namespace SmartStorePOS.Models
{
    public class GetProductRequest
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public GetProductQuery query { get; set; }
    }

    public class GetProductQuery
    {
        public string productId { get; set; }
        public string productNameQuery { get; set; }
        public string descriptionQuery { get; set; }
        public string productCode { get; set; }
        public string categoryCode { get; set; }
        public string[] categoryIds { get; set; }
        public double? minPrice { get; set; }
        public double? maxPrice { get; set; }
        public bool isSuspended { get; set; }
    }
}
