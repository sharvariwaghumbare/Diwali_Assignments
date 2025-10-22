namespace ECommerce.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        public string? ProductCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? StockQuantity { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
