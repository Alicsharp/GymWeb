namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class CategoryWithArticlesQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<ArticleListItemDto> Articles { get; set; } = new List<ArticleListItemDto>();
    }
}
