namespace Gtm.Contract.ArticleContract.Query
{
    public class BestArticleSliderQueryModel
    {
        public int Id { get; set; }
        public int VisitCount { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageName { get; set; }
        public string ImageAlt { get; set; }
    }
}
