using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.ProductFeaureDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class ProductFeatureConfig : IEntityTypeConfiguration<ProductFeature>
    {
        public void Configure(EntityTypeBuilder<ProductFeature> builder)
        {
            builder.ToTable("ProductFeatures");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.Value).IsRequired(true).HasMaxLength(155);

            builder.HasOne(o => o.Product).WithMany(s => s.ProductFeatures).HasForeignKey(s => s.ProductId);
        }
    }
}