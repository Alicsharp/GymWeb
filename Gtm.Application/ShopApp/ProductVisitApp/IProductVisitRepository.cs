using Gtm.Domain.ShopDomain.ProductVisitAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductVisitApp
{
    public interface IProductVisitRepository : IRepository<ProductVisit, int>
    {
        Task<bool> UbsertProductVisitAsymc(ProductVisit command);
        Task<int> GetTotalVisitCountAsync(CancellationToken cancellationToken = default);

    }
}
