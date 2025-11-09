using Gtm.Application.ShopApp.ProductApp;
using Gtm.Contract.ProductSellContract.Query;
using Gtm.Domain.ShopDomain.ProductDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductRepo
{
    internal class ProductRepository : Repository<Product, int>, IProductRepository
    {
        private readonly GtmDbContext _context;
        public ProductRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public IQueryable<Product> GetActiveProducts()
        {
        
 
            return _context.Products
                .Include(p => p.ProductSells).ThenInclude(s => s.OrderItems)
                .Include(p => p.ProductSells).ThenInclude(s => s.Seller)
                .Include(p => p.ProductCategoryRelations).ThenInclude(c => c.ProductCategory)
                .Where(p => p.IsActive && p.ProductSells.Count()>0);
        }
        public IQueryable<Product> GetActiveProductsWithSalesAndCategories()
        {
            var query = _context.Products
                .Include(p => p.ProductSells).ThenInclude(s => s.OrderItems)
                .Include(p => p.ProductSells).ThenInclude(s => s.Seller)
                .Include(p => p.ProductCategoryRelations).ThenInclude(c => c.ProductCategory)
                .Where(p => p.IsActive && p.ProductSells.Count() > 0)
                .OrderBy(p => p.Id);

            return query;
        }
        public async Task<Product?> GetBySlugWithCategoriesAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.ProductCategoryRelations)
                .FirstOrDefaultAsync(p => p.Slug.ToLower().Trim() == slug);
        }
        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.ProductCategoryRelations).SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetProductBySlugWithCategoryRelationsAsync(string productSlug)
        {
            return await _context.Products
             .Include(p => p.ProductCategoryRelations)
             .SingleOrDefaultAsync(p =>
                 p.Slug.Trim().ToLower() == productSlug.Trim().ToLower());
        }

        public async Task<Product> GetProductWithCategoryFeaturesGalleryAndActiveSellsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategoryRelations).ThenInclude(p => p.ProductCategory)
                .Include(p => p.ProductFeatures)
                .Include(p => p.ProductGalleries)
                .Include(p => p.ProductSells)
                    .ThenInclude(s => s.Seller)
                .SingleOrDefaultAsync(c => c.Id == id);
        }

        public IQueryable<Product> GetProductWithProductSellsAndOrderItems()
        {
            throw new NotImplementedException();
        }
        public async Task<List<ProductForAddProductSellQueryModel>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(c => c.ProductCategoryRelations)
                .Where(c => c.ProductCategoryRelations.Any(r => r.ProductCategoryId == categoryId))
                .Select(c => new ProductForAddProductSellQueryModel
                {
                    Id = c.Id,
                    Title = c.Title
                })
                .ToListAsync();
        }
        public IQueryable<Product> GetProductWithRelations()
        {
            return _context.Products.Include(p => p.ProductCategoryRelations);
        }
        public async Task<List<Product>> GetTop10BestSellingAsync()
        {
            // ما مرتب‌سازی و Take را قبل از ToListAsync می‌آوریم تا در دیتابیس اجرا شود
            return await _context.Products
                .Include(p => p.ProductSells).ThenInclude(s => s.OrderItems) // برای محاسبه Sum
                .Include(p => p.ProductSells).ThenInclude(s => s.Seller) // برای مپ کردن اولیه
                .Where(p => p.IsActive && p.ProductSells.Any())
                .OrderByDescending(p => p.ProductSells.Sum(s => s.OrderItems.Count)) // در دیتابیس اجرا می‌شود
                .Take(10) // در دیتابیس اجرا می‌شود
                .ToListAsync();
        }
        public async Task<List<Product>> GetTop10NewestAsync()
        {
            // تفاوت فقط در OrderByDescending است
            return await _context.Products
                .Include(p => p.ProductSells).ThenInclude(s => s.Seller) // برای مپ کردن اولیه
                .Where(p => p.IsActive && p.ProductSells.Any())
                .OrderByDescending(p => p.Id) // <-- تغییر یافته (بر اساس جدیدترین)
                .Take(10) // در دیتابیس اجرا می‌شود
                .ToListAsync();
        }
        public async Task<List<Product>> GetTop10MostVisitedAsync()
        {
            // تفاوت فقط در Include(ProductVisits) و OrderByDescending است
            return await _context.Products
                .Include(p => p.ProductVisits) // برای محاسبه Sum بازدید
                .Include(p => p.ProductSells).ThenInclude(s => s.Seller) // برای مپ کردن اولیه
                .Where(p => p.IsActive && p.ProductSells.Any())
                .OrderByDescending(p => p.ProductVisits.Sum(v => v.Count)) // <-- تغییر یافته (پربازدیدترین)
                .Take(10) // در دیتابیس اجرا می‌شود
                .ToListAsync();
        }
        public IQueryable<Product> SearchProductsByTitleQuery(string filter)
        {
            var filterText = filter.Trim().ToLower();

            // این متد کوئری را اجرا نمی‌کند، فقط آن را می‌سازد
            return _context.Products
                .Where(p => p.Title.ToLower().Contains(filterText));
        }
    }
}
