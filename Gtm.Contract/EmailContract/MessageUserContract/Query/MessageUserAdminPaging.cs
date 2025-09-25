
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.EmailContract.MessageUserContract.Query
{
    public class MessageUserAdminPaging : BasePaging
    {
        public List<MessageUserAdminQueryModel> Messages { get; set; }
        public string Filter { get; set; }
        public MessageStatus Status { get; set; }
    }
}
