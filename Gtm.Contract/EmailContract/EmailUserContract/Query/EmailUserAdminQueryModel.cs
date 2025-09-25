using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.EmailContract.EmailUserContract.Query
{
    public class EmailUserAdminQueryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CreationDate { get; set; }
        public bool Active { get; set; }
    }
}
