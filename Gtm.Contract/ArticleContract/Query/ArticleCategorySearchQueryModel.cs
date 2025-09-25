namespace Gtm.Contract.ArticleContract.Query
{
    public class ArticleCategorySearchQueryModel
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public int ArticleCount { get; set; }
        public int Id { get; set; }
        public List<ArticleCategorySearchQueryModel> Childs { get; set; }
    }
}
 
