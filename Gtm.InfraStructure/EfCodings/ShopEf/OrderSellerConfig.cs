using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class OrderSellerConfig : IEntityTypeConfiguration<OrderSeller>
    {
        public void Configure(EntityTypeBuilder<OrderSeller> builder)
        {
            builder.ToTable("OrderSellers");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Status).IsRequired();
            builder.Property(b => b.DiscountTitle).IsRequired(false).HasMaxLength(355);
            builder.HasMany(o => o.OrderItems).WithOne(s => s.OrderSeller).HasForeignKey(s => s.OrderSellerId);
            builder.HasOne(o => o.Order).WithMany(s => s.OrderSellers).HasForeignKey(s => s.OrderId);
        }
    }
}