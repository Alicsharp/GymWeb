using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gtm.InfraStructure.EfCodings.OrderDiscountEf
{
    internal class ProductDiscountConfig : IEntityTypeConfiguration<ProductDiscount>
    {
        public void Configure(EntityTypeBuilder<ProductDiscount> builder)
        {
            builder.ToTable("ProductDiscounts");
            builder.HasKey(x => x.Id);
        }
    }
}
