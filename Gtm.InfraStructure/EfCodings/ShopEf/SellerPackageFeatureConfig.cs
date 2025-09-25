using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.SellerPackageFeatureDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class SellerPackageFeatureConfig : IEntityTypeConfiguration<SellerPackageFeature>
    {
        public void Configure(EntityTypeBuilder<SellerPackageFeature> builder)
        {
            builder.ToTable("SellerPackageFeatures");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.Description).IsRequired(true);

            builder.HasOne(o => o.SellerPackage).WithMany(s => s.SellerPackageFeatures).HasForeignKey(s => s.SellerPackageId);
        }
    }
}