using Gtm.Domain.SiteDomain.SitePageAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Gtm.InfraStructure.EfCodings.SiteEf
{
    internal class SitePageConfig : IEntityTypeConfiguration<SitePage>
    {
        public void Configure(EntityTypeBuilder<SitePage> builder)
        {
            builder.ToTable("Pages");
            builder.HasKey(x => x.Id);
            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.Slug).IsRequired(true).HasMaxLength(355);
            builder.Property(b => b.Description).IsRequired(true);
        }
    }
}
