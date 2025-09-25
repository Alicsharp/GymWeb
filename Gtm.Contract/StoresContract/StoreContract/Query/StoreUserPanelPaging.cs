using Gtm.Contract.SellerContract.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.StoresContract.StoreContract.Query
{

    public class StoreUserPanelPaging : BasePaging
    {
        public int SellerId { get; set; }
        public string PageTitle { get; set; }
        public string Filter { get; set; }
        public List<StoreUserPanelQueryModel> Stores { get; set; }
        public List<SellerForStoresUserPanelQueryModel> Sellers { get; set; }
    }
}
