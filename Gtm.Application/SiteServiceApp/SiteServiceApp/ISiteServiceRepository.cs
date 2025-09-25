using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using Gtm.Domain.SiteDomain.SiteServiceAgg;

using Utility.Appliation.RepoInterface;


namespace Gtm.Application.SiteServiceApp.SiteServiceApp
{
    public interface ISiteServiceRepository : IRepository<SiteService,int>
    {
        Task<EditSiteService?> GetForEditAsync(int id);
    }
}
