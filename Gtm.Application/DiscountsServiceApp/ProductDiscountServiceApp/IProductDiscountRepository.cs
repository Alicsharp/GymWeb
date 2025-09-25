using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp
{
    public interface IProductDiscountRepository : IRepository<ProductDiscount, int> 
    {
        Task<ProductDiscount?> GetActiveDiscountForProductAsync(int productId, CancellationToken cancellationToken = default);
        Task<List<ProductDiscount>> GetActiveDiscountsForProductAsync(int productId, int productSellId = 0, CancellationToken cancellationToken = default);
    }
}
