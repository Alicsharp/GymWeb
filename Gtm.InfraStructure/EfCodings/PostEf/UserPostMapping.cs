using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
 
using Gtm.Domain.PostDomain.UserPostAgg;

namespace Gtm.InfraStructure.EfCodings.PostEf
{
    public class UserPostMapping : IEntityTypeConfiguration<UserPost>
    {
        public void Configure(EntityTypeBuilder<UserPost> builder)
        {
            builder.ToTable("UserPosts");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.ApiCode).IsRequired(true).HasMaxLength(50);
        }
    }
}
