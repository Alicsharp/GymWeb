using ErrorOr;
using Gtm.Application.StoresServiceApp.StoreProductApp;
using Gtm.Domain.StoresDomain.StoreProductAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.StoreRepo
{
    internal class StoreProductRepository : Repository<StoreProduct, int>, IStoreProductRepository
    {
        private readonly GtmDbContext _context;
        public StoreProductRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        //public async Task<ErrorOr<Success>> CreateListAsync(List<StoreProduct> storeProducts)
        //{
        //    await _context.StoreProducts.AddRangeAsync(storeProducts);
        //    if(await SaveChangesAsync())
        //    {
        //        return Result.Success;
        //    }
        //    return Error.Failure();

        //}
        public async Task CreateListAsync(IEnumerable<StoreProduct> storeProducts)
        {
            await _context.StoreProducts.AddRangeAsync(storeProducts);
            // اینجا SaveChanges نزن
        }

    }
}
