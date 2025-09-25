
using Gtm.Contract.SiteContract.SiteSettingContract.Command;
using Gtm.Domain.SiteDomain.SiteSettingAgg;
using Utility.Appliation.FileService;
using Utility.Appliation;
using Utility.Appliation.RepoInterface;
using ErrorOr;


namespace Gtm.Application.SiteServiceApp.SiteSettingApp
{
    public interface ISiteSettingRepository:IRepository<SiteSetting,int>
    {
        Task<UbsertSiteSetting> GetForUbsertAsync();
        Task<SiteSetting> GetSingleAsync();
        
    }
  
}
