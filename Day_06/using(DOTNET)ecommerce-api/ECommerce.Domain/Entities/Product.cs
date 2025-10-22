namespace ECommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string ProductCode { get; set; } // Unique code for the product, can be used for inventory management
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public int SoldQuantity { get; set; } = 0;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}
