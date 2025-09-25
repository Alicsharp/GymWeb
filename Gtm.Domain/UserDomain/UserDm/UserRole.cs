using Utility.Domain;

namespace Gtm.Domain.UserDomain.UserDm
{
    public class UserRole : BaseEntityCreate<int>
    {
        public int UserId { get; private set; }
        public int RoleId { get; private set; }
        public Role Role { get; private set; }
        public User User { get; private set; }

        public UserRole(int userId, int roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
