

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtm.Domain.EmailDomain.EmailUserAgg;

namespace Gtm.InfraStructure.EfCodings.EmailEf
{
    internal class EmailUserConfig : IEntityTypeConfiguration<EmailUser>
    {
        public void Configure(EntityTypeBuilder<EmailUser> builder)
        {
            builder.ToTable("EmailUsers");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Email).IsRequired(true).HasMaxLength(255);
        }
    }
}
