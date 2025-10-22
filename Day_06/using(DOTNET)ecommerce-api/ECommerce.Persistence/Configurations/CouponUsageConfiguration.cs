using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Configurations
{
    public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
    {
        public void Configure(EntityTypeBuilder<CouponUsage> builder)
        {
            builder.HasKey(cu => cu.Id);

            builder.HasOne(cu => cu.Coupon)
                .WithMany()
                .HasForeignKey(cu => cu.CouponId);

            builder.HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId);

            builder.Property(cu => cu.UsageCount).IsRequired();

        }
    }
}
