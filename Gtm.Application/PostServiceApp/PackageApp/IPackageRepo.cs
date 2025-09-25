using ErrorOr;
using Gtm.Contract.PostContract.UserPostContract.Command;
using Gtm.Domain.PostDomain.UserPostAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.PostServiceApp.PackageApp
{
    public interface IPackageRepo : IRepository<Package,int>
    {
        Task<EditPackage> GetForEditAsync(int id);
        Task<CreatePostOrder> GetCreatePostModelAsync(int userId, int packageId);
    }
  
}
