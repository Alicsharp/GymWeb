using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
 
using Gtm.Domain.PostDomain.SettingAgg;

namespace Gtm.InfraStructure.EfCodings.PostEf
{
    public class PostSettingMapping : IEntityTypeConfiguration<PostSetting>
    {
        public void Configure(EntityTypeBuilder<PostSetting> builder)
        {
            builder.ToTable("PostSettings");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.PackageTitle).IsRequired(false).HasMaxLength(255);
            builder.Property(b => b.PackageDescription).IsRequired(false);

        }
    }
}
