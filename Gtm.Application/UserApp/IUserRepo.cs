using Gtm.Domain.UserDomain.UserDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.UserApp
{
    public interface IUserRepo:IRepository<User,int>
    {
        Task<User?> GetByMobileAsync(string mobile);
        Task<List<User>> GetByIdsAsync(List<int> ids);

    }
}
