using Gtm.Domain.StoresDomain.StoreAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Domain;

namespace Gtm.Domain.StoresDomain.StoreProductAgg
{
    public class StoreProduct : BaseEntityCreate<int>
    {
        public int StoreId { get; private set; }
        public int ProductSellId { get; private set; }
        public StoreProductType Type { get; private set; }
        public int Count { get; private set; }
        public Store Store { get; private set; } // فقط Navigation، مقداردهی نکن

        public StoreProduct() { }

        public StoreProduct(int storeId, int productSellId, StoreProductType type, int count)
        {
            StoreId = storeId;
            ProductSellId = productSellId;
            Type = type;
            Count = count;
        }
    }
    

}
