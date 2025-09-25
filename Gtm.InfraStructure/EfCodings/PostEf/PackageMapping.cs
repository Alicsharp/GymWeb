using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
 
using Gtm.Domain.PostDomain.UserPostAgg;

namespace Gtm.InfraStructure.EfCodings.PostEf
{
    public class PackageMapping : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable("Packages");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.ImageName).IsRequired(true).HasMaxLength(150);
            builder.Property(b => b.ImageAlt).IsRequired(true).HasMaxLength(100);
            builder.Property(b => b.Description).IsRequired(true);

            builder.HasMany(p => p.PostOrders).WithOne(o => o.Package).HasForeignKey(p => p.PackageId);
        }
    }
}
