using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure.EfCodings.ArticleCategoryEf
{
    using Gtm.Domain.BlogDomain.BlogCategoryDm;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ArticleCategoryConfig : IEntityTypeConfiguration<ArticleCategory>
    {
        public void Configure(EntityTypeBuilder<ArticleCategory> builder)
        {
            // Table name
            builder.ToTable("ArticleCategories");

            builder.HasKey(x => x.Id);

            builder.Property(b => b.Title).IsRequired().HasMaxLength(250);
            builder.Property(b => b.Slug).IsRequired().HasMaxLength(300);
            builder.Property(b => b.ImageName).IsRequired().HasMaxLength(150);
            builder.Property(b => b.ImageAlt).IsRequired().HasMaxLength(150);
        }
    }
}
