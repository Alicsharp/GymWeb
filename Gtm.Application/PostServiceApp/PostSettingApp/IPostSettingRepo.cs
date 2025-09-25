using Gtm.Contract.PostContract.PostSettingContract.Command;
using Gtm.Domain.PostDomain.SettingAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.PostServiceApp.PostSettingApp
{
    public interface IPostSettingRepo : IRepository<PostSetting,int>
    {
        Task<UbsertPostSetting> GetForUbsertAsync();
        Task<PostSetting> GetSingleAsync();
    }
}
