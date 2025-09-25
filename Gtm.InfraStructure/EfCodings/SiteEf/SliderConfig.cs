using Gtm.Domain.SiteDomain.SliderAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gtm.InfraStructure.EfCodings.SiteEf
{
    internal class SliderConfig : IEntityTypeConfiguration<Slider>
    {
        public void Configure(EntityTypeBuilder<Slider> builder)
        {
            builder.ToTable("Sliders");
            builder.HasKey(x => x.Id);
            builder.Property(b => b.ImageName).IsRequired(true).HasMaxLength(155);
            builder.Property(b => b.ImageAlt).IsRequired(true).HasMaxLength(155);
            builder.Property(b => b.Url).IsRequired(true).HasMaxLength(900);
        }
    }
}
