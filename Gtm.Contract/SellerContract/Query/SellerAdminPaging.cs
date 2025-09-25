using Utility.Appliation;

namespace Gtm.Contract.SellerContract.Query
{
    public class SellerAdminPaging : BasePaging
    {
        public string Filter { get; set; }
        public List<SellerAdminQueryModel> Sellers { get; set; }
    }
}
