namespace Gtm.Contract.ProductCategoryContract.Query
{
    public class ProductCategoryUiQueryModel
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public List<ProductCategoryUiQueryModel> Childs { get; set; }
    }
}
