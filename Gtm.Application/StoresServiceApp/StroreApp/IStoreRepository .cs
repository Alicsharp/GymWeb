using Gtm.Contract.StoresContract.StoreContract.Query;
using Gtm.Domain.ShopDomain.SellerDomain;
using Gtm.Domain.StoresDomain.StoreAgg;
using Gtm.Domain.StoresDomain.StoreProductAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.StoresServiceApp.StroreApp
{
    public interface IStoreRepository : IRepository<Store, int>
    {
        Task<StoreUserPanelPaging> GetStoresForUserPanelAsync(int userId, int sellerId, string filter, int pageId = 1, int take = 10);
        Task<Store?> GetStoreWithProductsAsync(int storeId);
        Task<bool> VerifyStoreOwnershipAsync(int storeId, int userId);
        Task<int> CreateReturnKey(Store store);
        IQueryable<Store> GetStoresByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Seller?> GetSellerByIdAsync(int sellerId, CancellationToken cancellationToken = default);
        Task<List<Seller>> GetSellersByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Store> addstores(Store store);

    }
}
