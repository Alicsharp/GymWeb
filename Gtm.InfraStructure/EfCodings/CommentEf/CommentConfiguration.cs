using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Gtm.Domain.CommentDomain;

namespace Gtm.InfraStructure.EfCodings.CommentEf
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            // نام جدول در دیتابیس
            builder.ToTable("Comments", "dbo");

            // تعریف کلید اصلی
            builder.HasKey(c => c.Id);

            // پیکربندی فیلدها
            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.AuthorUserId)
                .IsRequired();

            builder.Property(c => c.TargetEntityId)
                .IsRequired();

            builder.Property(c => c.CommentFor)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(c => c.WhyRejected)
                .HasMaxLength(500);

            builder.Property(c => c.FullName)
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            builder.Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(1000);

            // روابط
            builder.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // یا DeleteBehavior.Cascade بسته به نیاز
            builder.HasOne(c => c.AuthorUser)
             .WithMany() // اگر در مدل User لیست کامنت‌ها رو نداری
              .HasForeignKey(c => c.AuthorUserId)
                  .OnDelete(DeleteBehavior.Restrict); // 
            // ایندکس‌ها
            builder.HasIndex(c => c.AuthorUserId);
            builder.HasIndex(c => c.TargetEntityId);
            builder.HasIndex(c => c.CommentFor);
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.ParentId);

            builder.Property(c => c.Status)
           .IsRequired()
           .HasConversion<int>()
           .HasDefaultValue(CommentStatus.خوانده_نشده);
            // عددی ذخیره می‌شود
            // پیکربندی تایم‌استمپ‌ها (اگر از BaseEntityCreate استفاده می‌کنید)
            builder.Property(c => c.CreateDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()"); // برای SQL Server
        }
    }
}
