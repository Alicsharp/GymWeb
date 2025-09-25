using ErrorOr;
using Gtm.Contract.ProductContract.Query;
using Gtm.Domain.ShopDomain.ProductSellDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductSellApp
{
    public interface IProductSellRepository:IRepository<ProductSell,int>
    {
        IQueryable<ProductSell> GetProductSellsForSeller(int sellerId, string? filter = null);
        IQueryable<ProductSell> GetSellerProductsWithDetailsAsync(int sellerId, string filter = null);
        Task<ErrorOr<Success>> IsProductSellForUserAsync(int userId, int id);
        Task<List<ProductForAddStoreQueryModel>> GetProductsForStoreCreation(int sellerId);
        Task< ProductSell> GetProductSellWithProductAsync(int productSellId);
        Task<List<ProductSell>> GetByIdsAsync(List<int> ids);
        Task<int> GetProductIdByProductSellIdAsync(int productSellId);
        Task<ProductSell?> GetProductSellByIdAsync(int productSellId, CancellationToken cancellationToken = default);
        Task<ProductSell> GetProductSellWithProductByIdAsync(int productSellId, CancellationToken cancellationToken);


    }
}
