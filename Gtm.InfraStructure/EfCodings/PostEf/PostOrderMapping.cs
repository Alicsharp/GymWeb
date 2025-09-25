using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
  using Gtm.Domain.PostDomain.UserPostAgg;

namespace Gtm.InfraStructure.EfCodings.PostEf
{
    public class PostOrderMapping : IEntityTypeConfiguration<PostOrder>
    {
        public void Configure(EntityTypeBuilder<PostOrder> builder)
        {
            builder.ToTable("PostOrders");
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.Package).WithMany(b => b.PostOrders).HasForeignKey(p => p.PackageId);
        }
    }
}
