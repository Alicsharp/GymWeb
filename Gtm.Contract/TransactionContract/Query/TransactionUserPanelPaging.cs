using Utility.Appliation;

namespace Gtm.Contract.TransactionContract.Query
{
    public class TransactionUserPanelPaging : BasePaging
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int WalletAmount { get; set; }
        public string Filter { get; set; }
        public List<TransactionUserPanelQueryModel> Transaction { get; set; }
    }
}
