using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.OrderDomain.OrderItemDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Unit).IsRequired(true).HasMaxLength(255);
            builder.HasOne(o => o.OrderSeller)
                .WithMany(s => s.OrderItems)
                .HasForeignKey(s => s.OrderSellerId)
                .OnDelete(DeleteBehavior.Cascade); // وقتی سفارش پاک شد آیتم‌هاش هم پاک شن

            builder.HasOne(o => o.ProductSell)
                .WithMany(s => s.OrderItems)
                .HasForeignKey(s => s.ProductSellId)
                .OnDelete(DeleteBehavior.Restrict); // محصول نباید cascade بشه
        }
    }

}