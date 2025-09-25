using Gtm.Contract.SiteContract.BanarContract.Command;
using Gtm.Domain.SiteDomain.BannerAgg;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.SiteServiceApp.BannerApp
{
    public interface IBanerRepository : IRepository<Baner,int>
    {
        Task<EditBaner?> GetForEditAsync(int id);
    }
}
