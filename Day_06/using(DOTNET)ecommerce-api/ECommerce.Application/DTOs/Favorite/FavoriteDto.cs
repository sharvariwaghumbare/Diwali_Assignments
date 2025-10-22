namespace ECommerce.Application.DTOs.Favorite
{
    public class FavoriteDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
