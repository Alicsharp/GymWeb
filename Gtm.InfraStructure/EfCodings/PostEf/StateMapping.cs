using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Gtm.Domain.PostDomain.StateAgg;


namespace Gtm.InfraStructure.EfCodings.PostEf
{
    public class StateMapping : IEntityTypeConfiguration<State>
    {
        public void Configure(EntityTypeBuilder<State> builder)
        {
            builder.ToTable("States");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title).IsRequired(true).HasMaxLength(155);
            builder.Property(b => b.CloseStates).IsRequired(false).HasMaxLength(90);

            builder.HasMany(b => b.Cities).
                WithOne(c => c.State)
                .HasForeignKey(c => c.StateId);
        }
    }
}
