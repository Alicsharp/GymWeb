using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.ShopDomain.ShopDomain;

namespace Gtm.InfraStructure.EfCodings.ShopEf
{
    internal class ShopSettingConfig : IEntityTypeConfiguration<ShopSetting>
    {
        public void Configure(EntityTypeBuilder<ShopSetting> builder)
        {
            builder.ToTable("ShopSettings");
            builder.HasKey(b => b.Id);

        }
    }
}