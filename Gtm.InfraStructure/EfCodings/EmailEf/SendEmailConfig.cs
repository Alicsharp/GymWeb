using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using Gtm.Domain.EmailDomain.SendEmailAgg;

namespace Gtm.InfraStructure.EfCodings.EmailEf
{
    internal class SendEmailConfig : IEntityTypeConfiguration<SendEmail>
    {
        public void Configure(EntityTypeBuilder<SendEmail> builder)
        {
            builder.ToTable("SendEmails");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(255);
            builder.Property(b => b.Text).IsRequired(true);
        }
    }
}
