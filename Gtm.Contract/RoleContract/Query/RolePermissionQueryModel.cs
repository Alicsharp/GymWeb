using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.RoleContract.Query
{
    public class RolePermissionQueryModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public UserPermission UserPermission { get; set; }
    }
}
