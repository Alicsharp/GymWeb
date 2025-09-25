using Gtm.Domain.UserDomain.UserDm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gtm.InfraStructure.EfCodings.UserEf
{
    internal class PermissionConfig : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserPermission).IsRequired();

            builder.HasOne(b => b.Role)
                .WithMany(a => a.Permissions).HasForeignKey(a => a.RoleId);

        }
    }
}
