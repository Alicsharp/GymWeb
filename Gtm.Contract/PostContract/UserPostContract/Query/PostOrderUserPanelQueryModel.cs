using Utility.Domain.Enums;

namespace Gtm.Contract.PostContract.UserPostContract.Query
{
    public class PostOrderUserPanelQueryModel
    {

        public int Id { get; set; }
        public int PackageId { get; set; }
        public string PackageTitle { get; set; }
        public string PackageImage { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
        public string Date { get; set; }
        public int transactionId { get; set; }
        public string TransactionRef { get; set; }
        public PostOrderStatus Status { get; set; }
    }
}
 