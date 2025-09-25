using Gtm.Contract.SiteContract.SitePageContract.Command;
using Gtm.Domain.SiteDomain.SitePageAgg;
using Utility.Appliation.RepoInterface;


namespace Gtm.Application.SiteServiceApp.SitePageApp
{
    public interface ISitePageRepository : IRepository<SitePage,int>
    {
        Task<EditSitePage?> GetForEditAsync(int id);
        Task<SitePage> GetBySlugAsync(string slug);
    }
}
