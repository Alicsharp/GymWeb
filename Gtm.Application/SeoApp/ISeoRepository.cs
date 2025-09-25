
using ErrorOr;
using Gtm.Contract.SeoContract.Command;
using Gtm.Domain.SeoDomain;
using System.Drawing;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;

namespace Gtm.Application.SeoApp
{
    public interface ISeoRepository : IRepository<Seo, int>
    {
        Task<CreateSeo> GetSeoForUpsertAsync(int ownerId, WhereSeo where);
        Task<Seo?> GetSeoAsync(int ownerId, WhereSeo where);
        Task<Seo?> GetSeoForUiAsync(int ownerId, WhereSeo where, string title);
    }


}
