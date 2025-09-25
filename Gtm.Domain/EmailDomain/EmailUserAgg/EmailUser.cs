using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.EmailDomain.EmailUserAgg
{
    public class EmailUser : BaseEntityCreateActive<int>
    {
        public int UserId { get; private set; }
        public string Email { get; private set; }

        public EmailUser(int userId, string email)
        {
            UserId = userId;
            Email = email;
        }
        public void AddUserId(int userId)
        {
            UserId = userId;
        }
        public void EdiEmail(string email)
        {
            Email = email;
        }

    }
}
