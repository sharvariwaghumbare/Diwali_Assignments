using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.FullAddress)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(20);
            builder.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
