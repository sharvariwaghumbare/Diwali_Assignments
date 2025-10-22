namespace ECommerce.Application.DTOs.Product
{
    public class ProductFilterDto
    {
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
