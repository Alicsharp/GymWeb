using Gtm.Domain.UserDomain.UserDm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure.EfCodings.UserEf
{
    internal class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", "user"); // نام جدول و اسکیمای آن

            // تنظیم کلید اصلی
            builder.HasKey(u => u.Id);

            // تنظیمات خصوصیات
            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired(false); // در صورتی که الزامی نیست

            builder.Property(u => u.Mobile)
                .HasMaxLength(11)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.Password)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Avatar)
                .HasMaxLength(200)
                .HasDefaultValue("default.png");

            builder.Property(u => u.IsDelete)
                .HasDefaultValue(false);

            builder.Property(u => u.UserGender)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.HasMany(u => u.UserAddresses)
                 .WithOne(ua => ua.User) // اضافه کردن navigation property
                 .HasForeignKey(ua => ua.UserId)
                    .OnDelete(DeleteBehavior.ClientCascade); // تغییر به ClientCascade

            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Wallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
