using Utility.Appliation;

namespace Gtm.Contract.UserContract.Query
{
    public class UsersForAdminPaging : BasePaging
    {
        public string Filter { get; set; }
        public UserStatusSearch Status { get; set; }
        public UserOrderBySearch OrderBy { get; set; }
        public List<UserForAdminQueryModel> Users { get; set; }
    }
}
