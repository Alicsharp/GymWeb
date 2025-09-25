using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class OrderAddressConfig : IEntityTypeConfiguration<OrderAddress>
    {
        public void Configure(EntityTypeBuilder<OrderAddress> builder)
        {
            builder.ToTable("OrderAddresses");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.IranCode).IsRequired(false).HasMaxLength(10);
            builder.Property(b => b.AddressDetail).IsRequired(true).HasMaxLength(600);
            builder.Property(b => b.FullName).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.PostalCode).IsRequired(true).HasMaxLength(10);
            builder.Property(b => b.Phone).IsRequired(true).HasMaxLength(11);

        }
    }
}