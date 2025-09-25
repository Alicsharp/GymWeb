namespace Gtm.Contract.ArticleCategoryContract.Command
{
    public class DeleteArticleCategoryDto
    {
        public int Id { get; set; }
        public bool ForceDelete { get; set; }
        public string Reason { get; set; }
    }
}
