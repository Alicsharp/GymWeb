using Utility.Domain;
using Utility.Domain.Enums;

namespace Gtm.Domain.UserDomain.UserDm
{
    public class Permission : BaseEntityCreate<int>
    {
        public int RoleId { get; private set; }
        public UserPermission UserPermission { get; private set; }
        public Role Role { get; private set; }
        public Permission(int roleId, UserPermission userPermission)
        {
            RoleId = roleId;
            UserPermission = userPermission;
        }
    }
}
