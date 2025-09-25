namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class ArticleCategoryAdminQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public string CreationDate { get; set; }
        public string UpdateDate { get; set; }
        public bool Active { get; set; }
    }
}
