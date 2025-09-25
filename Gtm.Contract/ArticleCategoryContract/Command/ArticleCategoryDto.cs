namespace Gtm.Contract.ArticleCategoryContract.Command
{
    public class ArticleCategoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageName { get; set; }
        public string ImageAlt { get; set; }
        public int? ParentId { get; set; }
        public bool IsActive { get; set; }
    }
}
