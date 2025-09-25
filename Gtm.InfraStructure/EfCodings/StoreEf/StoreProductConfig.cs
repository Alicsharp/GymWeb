using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.StoresDomain.StoreProductAgg;

namespace Gtm.InfraStructure.EfCodings.StoreEf
{
    internal class StoreProductConfig : IEntityTypeConfiguration<StoreProduct>
    {
        public void Configure(EntityTypeBuilder<StoreProduct> builder)
        {
            builder.ToTable("StoreProducts");
            builder.HasKey(x => x.Id);
            builder.Property(b => b.Type).IsRequired();

            builder.HasOne(b => b.Store).WithMany(s => s.StoreProducts).HasForeignKey(s => s.StoreId);
        }
    }
}
