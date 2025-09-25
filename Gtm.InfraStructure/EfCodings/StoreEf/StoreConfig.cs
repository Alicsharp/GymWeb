using Gtm.Domain.StoresDomain.StoreAgg;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure.EfCodings.StoreEf
{
    internal class StoreConfig : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Stores");
            builder.HasKey(x => x.Id);
            builder.Property(b => b.Description).IsRequired();

            builder.HasMany(b => b.StoreProducts).WithOne(s => s.Store).HasForeignKey(s => s.StoreId);
        }
    }
}
