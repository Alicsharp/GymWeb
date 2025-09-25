using Utility.Domain;


namespace Gtm.Domain.BlogDomain.BlogDm
{
    public class Tag : BaseEntityCreateUpdateActive<int>
    {
        public string Name { get; private set; }
        public string Slug { get; private set; }

        // Navigation Property
        public virtual ICollection<ArticleTag> ArticleTags { get; private set; } = new HashSet<ArticleTag>();

        protected Tag() { } // For EF Core

        public Tag(string name, string slug)
        {
            Validate(name, slug);

            Name = name;
            Slug = slug.ToLowerInvariant();
        }

        public void Update(string name, string slug)
        {
            Validate(name, slug);

            Name = name;
            Slug = slug.ToLowerInvariant();

        }

        private void Validate(string name, string slug)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Tag slug cannot be empty", nameof(slug));
        }
    }

}
