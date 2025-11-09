using Gtm.Contract.ProductSellContract.Query;
using Gtm.Domain.ShopDomain.ProductDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductApp
{
    public interface IProductRepository : IRepository<Product, int>
    {
        Task<Product> GetProductByIdAsync(int id);
        IQueryable<Product> GetProductWithRelations();
        IQueryable<Product> GetActiveProductsWithSalesAndCategories();
        Task<Product> GetProductBySlugWithCategoryRelationsAsync(string productSlug);
        Task<Product> GetProductWithCategoryFeaturesGalleryAndActiveSellsAsync(int id);
        Task<List<ProductForAddProductSellQueryModel>> GetProductsByCategoryIdAsync(int categoryId);
        IQueryable<Product> GetActiveProducts();
        Task<Product?> GetBySlugWithCategoriesAsync(string slug);
        Task<List<Product>> GetTop10BestSellingAsync();
   
        Task<List<Product>> GetTop10NewestAsync();
     
        Task<List<Product>> GetTop10MostVisitedAsync();
        IQueryable<Product> SearchProductsByTitleQuery(string filter);
      
    }
}
