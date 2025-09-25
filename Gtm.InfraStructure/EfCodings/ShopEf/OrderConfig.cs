using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.OrderDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.OrderStatus).IsRequired();
            builder.Property(b => b.OrderPayment).IsRequired();
            builder.Property(b => b.PostTitle).IsRequired(false).HasMaxLength(600);
            builder.Property(b => b.DiscountTitle).IsRequired(false).HasMaxLength(355);
            builder.HasMany(o => o.OrderSellers).WithOne(s => s.Order).HasForeignKey(s => s.OrderId);
        }
    }
}