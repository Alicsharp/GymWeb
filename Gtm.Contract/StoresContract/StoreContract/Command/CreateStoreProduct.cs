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

        public int ProductSellId { get; private set; }
        public StoreProductType Type { get; private set; }
        public int Count { get; private set; }
    }
}
