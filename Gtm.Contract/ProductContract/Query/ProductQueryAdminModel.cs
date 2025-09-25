namespace Gtm.Contract.ProductContract.Query
{
    public class ProductQueryAdminModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageName { get; set; }
        public string ImageAlt { get; set; }
        public int Weight { get; set; }
        public bool Active { get; set; }
        public string CreationDate { get; set; }
        public string UpdateDate { get; set; }
    } 
}
