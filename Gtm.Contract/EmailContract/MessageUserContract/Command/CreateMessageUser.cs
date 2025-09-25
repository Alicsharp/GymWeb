using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.EmailContract.MessageUserContract.Command
{
    public class CreateMessageUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Subject { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string Message { get; set; }
    }
}
