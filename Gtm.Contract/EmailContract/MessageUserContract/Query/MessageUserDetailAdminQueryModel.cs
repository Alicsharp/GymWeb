
using Utility.Domain.Enums;

namespace Gtm.Contract.EmailContract.MessageUserContract.Query
{
    public class MessageUserDetailAdminQueryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UseName { get; set; }
        public MessageStatus Status { get; set; }
        public string FullName { get; set; }
        public string Subject { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string CreationDate { get; set; }
        public string Message { get; set; }
        public string? AnswerSms { get; set; }
        public string? AnswerEmail { get; set; }
    }
}
