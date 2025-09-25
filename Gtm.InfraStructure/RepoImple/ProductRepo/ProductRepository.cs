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
        .Include(p => p.ProductCategoryRelations)
            .ThenInclude(p => p.ProductCategory)
        .Include(p => p.ProductFeatures)
        .Include(p => p.ProductGalleries)
        .Include(p => p.ProductSells.Where(s => s.IsActive))
        .SingleOrDefaultAsync(p => p.Id == id);
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
    }
}
