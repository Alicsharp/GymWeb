using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.RoleContract.Query
{
    public class RoleQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<UserRoleQueryModel> UserRoles { get; set; }
    }
}
