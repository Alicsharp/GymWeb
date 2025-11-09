using Utility.Domain.Enums;

namespace Gtm.Contract.AdminDashboard
{
    public class LastOrderAdminQueryModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public OrderStatus Status { get; set; }
        public int PaymentPrice { get; set; }
        public string CreationDate { get; set; }
    }

}
