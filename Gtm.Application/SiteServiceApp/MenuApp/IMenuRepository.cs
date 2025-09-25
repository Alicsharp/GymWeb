using Gtm.Contract.SiteContract.MenuContract.Command;
using Gtm.Domain.SiteDomain.MenuAgg;

using Utility.Appliation.RepoInterface;


namespace Gtm.Application.SiteServiceApp.MenuApp
{
    public interface IMenuRepository : IRepository<Menu,int>
    {
        Task<EditMenu?> GetForEditAsync(int id);
    }
}
