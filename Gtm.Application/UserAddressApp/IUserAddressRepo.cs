using Gtm.Domain.UserDomain.UserDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.UserAddressApp
{
    public interface IUserAddressRepo:IRepository<UserAddress,int>
    {
    }
}
