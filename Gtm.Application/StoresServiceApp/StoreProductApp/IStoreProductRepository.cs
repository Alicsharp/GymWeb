using ErrorOr;
using Gtm.Domain.StoresDomain.StoreAgg;
using Gtm.Domain.StoresDomain.StoreProductAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.StoresServiceApp.StoreProductApp
{
    public interface IStoreProductRepository : IRepository<StoreProduct, int>
    {
        //Task<ErrorOr<Success>> CreateListAsync(List<StoreProduct> storeProducts);
        Task CreateListAsync(IEnumerable<StoreProduct> storeProducts);
        


    }
}
