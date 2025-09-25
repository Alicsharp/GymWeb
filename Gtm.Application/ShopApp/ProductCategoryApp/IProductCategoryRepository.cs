using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.ProductCategoryContract.Query;
using Gtm.Contract.ProductContract.Query;
using Gtm.Domain.ShopDomain.ProductCategoryDomain;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductCategoryApp
{
    public interface IProductCategoryRepository : IRepository<ProductCategory, int>
    {
        Task<bool> CheckProductCategoriesExist(List<int> categoryids);
        Task<List<ProductCategoryUiQueryModel>> GetActiveCategoriesWithHierarchyAsync();
        Task<List<BreadCrumbQueryModel>> GenerateBreadcrumbsAsync(int? categoryId);
        Task<ProductCategory> FindProductCategoryByIdAsync(int categoryId);
        Task< ProductCategory> GetParentCategoryByIdAsync(int parentCategoryId);
        Task<ProductCategory?> GetCategoryBySlugAsync(string slug);
        IQueryable<ProductCategory?> GetActiveCategoriesAsync();
    }
}
