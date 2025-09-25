namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class ArticleCategoryForAddArticleQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<ArticleCategoryForAddArticleQueryModel> SubCategories { get; set; }
    }
}
