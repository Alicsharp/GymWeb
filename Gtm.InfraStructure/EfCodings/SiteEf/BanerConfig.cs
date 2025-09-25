using Gtm.Domain.SiteDomain.BannerAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure.EfCodings.SiteEf
{
    internal class BanerConfig : IEntityTypeConfiguration<Baner>
    {
        public void Configure(EntityTypeBuilder<Baner> builder)
        {
            builder.ToTable("Baners");
            builder.HasKey(x => x.Id);
            builder.Property(b => b.State).IsRequired(true);
            builder.Property(b => b.Url).IsRequired(true).HasMaxLength(900);
            builder.Property(b => b.ImageName).IsRequired(true).HasMaxLength(155);
            builder.Property(b => b.ImageAlt).IsRequired(true).HasMaxLength(155);
        }
    }
}
