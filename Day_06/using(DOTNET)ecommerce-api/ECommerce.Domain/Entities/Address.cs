using ECommerce.Domain.Identity;

namespace ECommerce.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }  // e.g., "Home", "Work"
        public string FullAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public bool IsDeleted { get; set; } = false;

        public AppUser User { get; set; }
    }
}
