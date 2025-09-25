using Gtm.Application.UserAddressApp;
using Gtm.Domain.UserDomain.UserDm;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.UserRepo
{
    internal class UserAddressRepository : Repository<UserAddress,int>, IUserAddressRepo
    {
        private readonly GtmDbContext _context;
        public UserAddressRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
