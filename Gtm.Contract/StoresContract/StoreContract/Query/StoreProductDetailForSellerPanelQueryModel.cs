using Utility.Domain.Enums;

namespace Gtm.Contract.StoresContract.StoreContract.Query
{
    public class StoreProductDetailForSellerPanelQueryModel
    {
        public int Id { get; set; }
        public int ProductSellId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string Unit { get; set; }
        public StoreProductType Type { get; set; }
        public int Count { get; set; }
        public string ProductImageName { get; set; }
    }
}
