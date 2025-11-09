using Utility.Domain.Enums;

namespace Gtm.Contract.StoresContract.StoreContract.Command
{
    public class CreateStoreProduct
    {
        public CreateStoreProduct(int productSellId, StoreProductType type, int count)
        {
            ProductSellId = productSellId;
            Type = type;
            Count = count;
        }
        public CreateStoreProduct()
        {

        }
        public int ProductSellId { get; set; }
        public StoreProductType Type { get; set; }
        public int Count { get; set; }
    }

}
