using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtm.Domain.TransactionDomian;

namespace Gtm.InfraStructure.EfCodings.TransactionEd
{
    internal class TranactionConfig : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.RefId).IsRequired(false).HasMaxLength(100);

            builder.Property(b => b.Portal).IsRequired();
            builder.Property(b => b.TransactionFor).IsRequired();
            builder.Property(b => b.Status).IsRequired();

        }
    }
}
