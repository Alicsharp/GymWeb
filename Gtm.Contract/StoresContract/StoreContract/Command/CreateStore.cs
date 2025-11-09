using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.StoresContract.StoreContract.Command
{
    public class CreateStore
    {
        public CreateStore(int sellerId, string description, List<CreateStoreProduct> products)
        {
            SellerId = sellerId;
            Description = description;
            Products = products;
        }
        public CreateStore()
        {

        }
        public int SellerId { get; set; }
        public string Description { get; set; }
        public List<CreateStoreProduct> Products { get; set; }
    }
}
