 
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtm.Domain.PostDomain.CityAgg;

namespace Gtm.InfraStructure.EfCodings.PostEf
{
    public class CityMapping : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.Status).IsRequired(true);
            builder.HasOne(b => b.State).WithMany(s => s.Cities).
                HasForeignKey(b => b.StateId);
        }
    }
}
