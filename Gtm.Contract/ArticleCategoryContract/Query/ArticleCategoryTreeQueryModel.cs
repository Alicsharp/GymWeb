namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class ArticleCategoryTreeQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public List<ArticleCategoryTreeQueryModel> Children { get; set; } = new List<ArticleCategoryTreeQueryModel>();
    }
}
