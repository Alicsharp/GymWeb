using Utility.Domain.Enums;

namespace Gtm.WebApp.Models
{
    public class OnlinePaymentViewModel
    {
        public bool Success { get; set; }
        public string RefId { get; set; }
        public string Description { get; set; }
        public TransactionFor TransactionFor { get; set; }
        public int OwnerId { get; set; }
        public int Price { get; set; }
    }
}
