using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.EmailContract.EmailUserContract.Command
{
    public class CreateEmailUser
    {
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}
