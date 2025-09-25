using Gtm.Contract.SellerContract.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.ProductSellContract.Query
{
    public class SellerProductPageUserPanelQueryModel : BasePaging
    {
        public string Filter { get; set; }
        public int SellerId { get; set; }
        public string SellerTitle { get; set; }
        public List<ProductSellUserPanelQueryModel> Products { get; set; }
    }
}
