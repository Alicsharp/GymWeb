using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
 
using Gtm.Domain.PostDomain.Postgg;

namespace Gtm.InfraStructure.EfCodings.PostEf
{
    internal class PostMapping : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired().HasMaxLength(250);
            builder.Property(b => b.Status).IsRequired(false).HasMaxLength(450);
            builder.Property(b => b.Description).IsRequired(false);

            builder.HasMany(b => b.PostPrices).
                WithOne(p => p.Post).
                HasForeignKey(p => p.PostId);
        }
    }
}
