using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.OrderDiscountRepo
{
    internal class ProductDiscountRepository : Repository<ProductDiscount,int>, IProductDiscountRepository
    {
        private readonly GtmDbContext _context;
        public ProductDiscountRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<ProductDiscount?> GetActiveDiscountForProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            return await _context.ProductDiscounts
                .Where(d => d.ProductId == productId && d.StartDate <= now && d.EndDate >= now)
                .OrderByDescending(d => d.StartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<List<ProductDiscount>> GetActiveDiscountsForProductAsync(int productId,int productSellId = 0,CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;

            return await _context.ProductDiscounts
                .Where(d =>
                    d.StartDate <= now &&
                    d.EndDate >= now &&
                    (d.ProductId == productId || (productSellId > 0 && d.ProductSellId == productSellId))
                )
                .OrderBy(d => d.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
