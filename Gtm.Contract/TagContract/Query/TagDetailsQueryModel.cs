using Gtm.Contract.ArticleCategoryContract.Query;

namespace Gtm.Contract.TagContract.Query
{
    public class TagDetailsQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsActive { get; set; }
        public List<ArticleListItemDto> Articles { get; set; } = new List<ArticleListItemDto>();
    }
}
