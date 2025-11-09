using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.ProductContract.Query;
using Gtm.Domain.ShopDomain.ProductSellDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductSellRepo
{
    public class ProductSellRepository:Repository<ProductSell,int>,IProductSellRepository
    {
        private readonly GtmDbContext _context;
        public ProductSellRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<ProductSell>> GetByIdsAsync(List<int> ids)
        {
            // اگر لیست خالی بود، لیست خالی برگردان
            if (ids == null || !ids.Any())
                return new List<ProductSell>();

            // کوئری واحد با WHERE Id IN (...)
            return await _context.ProductSells
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }
        public async Task<ProductSell> GetProductSellWithProductByIdAsync(int productSellId, CancellationToken cancellationToken)
        {
            return await _context.ProductSells
                .Include(ps => ps.Product)
                .SingleAsync(ps => ps.Id == productSellId, cancellationToken);
        }
        public async Task<ProductSell?>  GetProductSellByIdAsync(int productSellId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductSells
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.Id == productSellId, cancellationToken);
        }
        public   IQueryable<ProductSell> GetSellerProductsWithDetailsAsync(int sellerId, string filter = null)
        {
            var query = _context.ProductSells
                .Include(s => s.OrderItems)
                    .ThenInclude(o => o.OrderSeller)
                .Include(s => s.Product)
                .Where(s => s.SellerId == sellerId);

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(r => r.Product.Title.Contains(filter));
            }

            return query;
        }
        public async Task<ProductSell> GetProductSellWithProductAsync(int productSellId)
        {

            var productSell = await _context.ProductSells
             .Include(ps => ps.Product).SingleOrDefaultAsync(ps => ps.Id == productSellId);

            if (productSell == null)
                throw new Exception("ProductSell not found!"); // یا مدیریت مناسب دیگر
            return productSell;
        }
        public async Task<List<ProductForAddStoreQueryModel>> GetProductsForStoreCreation(int sellerId)
        {
            return await _context.ProductSells
           .Include(s => s.Product)
           .Where(p => p.SellerId == sellerId)
           .Select(p => new ProductForAddStoreQueryModel
           {
               Price = p.Price,
               ProductId = p.ProductId,
               ProductSellId = p.Id,
               ProductTitle = p.Product.Title,
               Unit = p.Unit
           })
           .ToListAsync();
        }
        public IQueryable<ProductSell> GetProductSellsForSeller(int sellerId, string? filter = null)
        {
            var query = _context.ProductSells
                .Include(s => s.OrderItems)
                    .ThenInclude(o => o.OrderSeller)
                .Include(s => s.Product)
                .Where(s => s.SellerId == sellerId);

            if (!string.IsNullOrEmpty(filter))
                query = query.Where(r => r.Product.Title.Contains(filter));

            return query;
        }
        public async Task<ErrorOr<Success>> IsProductSellForUserAsync(int userId, int id)
        {
            var productSell = await _context.ProductSells
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(c => c.Seller.UserId == userId && c.Id == id);

            if (productSell is null)
            {
                return Error.NotFound(
                    code: "ProductSell.NotFound",
                    description: "محصول یافت نشد یا متعلق به کاربر نیست");
            }

            return Result.Success;
        }

        public async Task<int> GetProductIdByProductSellIdAsync(int productSellId)
        {
            var sell = await _context.ProductSells.FindAsync(productSellId);
            return sell.ProductId;
        }

        public async Task<ProductSell> GetById(int i)
        {
        return    await _context.ProductSells.FindAsync(i);
        }
        public async Task<ProductSell> GetProductSellWithProductAsync(int productSellId, CancellationToken cancellationToken = default)
        {
            // این دقیقاً همان کدی است که شما ارائه کردید
            var productSell = await _context.ProductSells
                .Include(p => p.Product)
                .SingleOrDefaultAsync(ps => ps.Id == productSellId, cancellationToken);

            return productSell;
        }
        public async Task<List<ProductSell>> GetSellsWithSellerAsync(List<int> productSellIds)
        {
            return await _context.ProductSells
                .Include(ps => ps.Seller)
                .Where(ps => productSellIds.Contains(ps.Id))
                .ToListAsync();
        }
        public async Task<ProductSell?> GetWithProductAsync(int productSellId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductSells
                .Include(ps => ps.Product)
                .AsNoTracking()
                .SingleOrDefaultAsync(ps => ps.Id == productSellId, cancellationToken);
        }
    }
}
