using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.ProductCategoryDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class ProductCategoryConfig : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.Slug).IsRequired(true).HasMaxLength(355);
            builder.Property(b => b.ImageName).IsRequired(true).HasMaxLength(155);
            builder.Property(b => b.ImageAlt).IsRequired(true).HasMaxLength(155);

            builder.HasMany(o => o.ProductCategoryRelations).WithOne(s => s.ProductCategory).HasForeignKey(s => s.ProductCategoryId);
        }
    }
}