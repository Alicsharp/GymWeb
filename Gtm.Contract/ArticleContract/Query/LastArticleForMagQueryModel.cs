namespace Gtm.Contract.ArticleContract.Query
{
    public class LastArticleForMagQueryModel
    {
        public LastArticleForMagQueryModel(string slug, string title)
        {
            Slug = slug;
            Title = title;
        }

        public string Slug { get; private set; }
        public string Title { get; private set; }
    }
}
