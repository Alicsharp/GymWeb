using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.StoresContract.StoreContract.Query
{
    public class StoreDetailForSellerPanelQueryModel
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public string SellerTitle { get; set; }
        public string Description { get; set; }
        public string CreationDate { get; set; }
        public List<StoreProductDetailForSellerPanelQueryModel> StoreProducts { get; set; }
    }
}
