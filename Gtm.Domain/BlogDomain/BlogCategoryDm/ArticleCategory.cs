using Gtm.Domain.BlogDomain.BlogDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.BlogDomain.BlogCategoryDm
{
    // در پوشه Domain/Blog
    public class ArticleCategory : BaseEntityCreateUpdateActive<int>
    {
        public string Title { get; private set; }
        public string Slug { get; private set; }
        public string ImageName { get; private set; }
        public string ImageAlt { get; private set; }
        public int? ParentId { get; private set; }
        public ArticleCategory? Parent { get; private set; }
        public ICollection<ArticleCategory> Children { get; private set; } = new List<ArticleCategory>();
        public ICollection<Article> Articles { get; private set; } = new List<Article>();

        protected ArticleCategory() { } // برای EF Core

        public ArticleCategory(string title, string slug, string imageName, string imageAlt, int? parentId = null)
        {
            SetValues(title, slug, imageName, imageAlt);
            ParentId = parentId;

        }

        public void Edit(string title, string slug, string imageName, string imageAlt)
        {
            SetValues(title, slug, imageName, imageAlt);

        }

        public void ChangeParent(int? newParentId)
        {
            ParentId = newParentId;

        }
        public void Update(string title, string imageName, string imageAlt, string metaDescription, int? parentId)
        {
            Title = title;
            ImageName = imageName;
            ImageAlt = imageAlt;
            ParentId = parentId;

            UpdateEntity();
        }

        private void SetValues(string title, string slug, string imageName, string imageAlt)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));

            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty", nameof(slug));

            Title = title.Trim();
            Slug = slug.Trim().ToLowerInvariant();
            ImageName = imageName?.Trim();
            ImageAlt = imageAlt?.Trim();
        }

    }
}

