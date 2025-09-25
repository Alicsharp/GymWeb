using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class ProductGalleryConfig : IEntityTypeConfiguration<ProductGallery>
    {
        public void Configure(EntityTypeBuilder<ProductGallery> builder)
        {
            builder.ToTable("ProductGalleries");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.ImageAlt).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.ImageName).IsRequired(true).HasMaxLength(155);

            builder.HasOne(o => o.Product).WithMany(s => s.ProductGalleries).HasForeignKey(s => s.ProductId);
        }
    }
}