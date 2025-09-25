using Utility.Domain;


namespace Gtm.Domain.BlogDomain.BlogDm
{
    public class ArticleTag : BaseEntity<int>
    {
        // Foreign Keys
        public int ArticleId { get; private set; }
        public int TagId { get; private set; }

        // Navigation Properties
        public virtual Article Article { get; private set; }
        public virtual Tag Tag { get; private set; }

        // Constructor for EF Core
        protected ArticleTag() { }

        public ArticleTag(int articleId, int tagId)
        {
            ArticleId = articleId;
            TagId = tagId;
        }

        // Factory Method for better semantics
        public static ArticleTag Create(int articleId, int tagId)
        {
            return new ArticleTag(articleId, tagId);
        }
    }

}
