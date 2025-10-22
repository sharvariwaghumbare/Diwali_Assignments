using ECommerce.Domain.Identity;

namespace ECommerce.Domain.Entities
{
    public class Favorite : BaseEntity
    {
        public Guid UserId { get; set; }
        public AppUser User { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }

}
