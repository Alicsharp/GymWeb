using Gtm.Domain.DiscountsDomain.OrderDiscount;
using Gtm.Domain.ShopDomain.OrderDomain.OrderItemDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure.EfCodings.OrderDiscountEf
{
    internal class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Unit).IsRequired(true).HasMaxLength(255);
            builder.HasOne(o => o.OrderSeller).WithMany(s => s.OrderItems).HasForeignKey(s => s.OrderSellerId);
            builder.HasOne(o => o.ProductSell).WithMany(s => s.OrderItems).HasForeignKey(s => s.ProductSellId);
        }
    }
}
