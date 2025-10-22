using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime? LastFailedLogin { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}
