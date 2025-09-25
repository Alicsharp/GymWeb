using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Domain.DiscountsDomain.OrderDiscount;
using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.OrderDiscountRepo
{
    internal class OrderDiscountRepository : Repository<OrderDiscount, int>, IOrderDiscountRepository
    {
        private readonly GtmDbContext _context;
        public OrderDiscountRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<OrderDiscount> GetByCodeAsync(string code) =>
      await _context.OrderDiscounts.SingleOrDefaultAsync(s => s.Code.Trim() == code.Trim());
        public async Task<ProductDiscount?> GetActiveDiscountByProductIdAsync(int productId, CancellationToken cancellationToken)
        {
            return await _context.ProductDiscounts
                .Where(p => p.ProductId == productId &&
                            p.StartDate.Date <= DateTime.Now.Date &&
                            p.EndDate.Date >= DateTime.Now.Date)
                .OrderByDescending(p => p.Percent)
                .FirstOrDefaultAsync(cancellationToken);
        }

    }
}
