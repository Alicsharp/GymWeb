namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class ArticleListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
