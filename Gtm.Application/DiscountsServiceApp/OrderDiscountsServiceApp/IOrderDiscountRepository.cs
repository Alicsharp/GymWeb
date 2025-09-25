using Gtm.Domain.DiscountsDomain.OrderDiscount;
using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp
{
    public interface IOrderDiscountRepository : IRepository<OrderDiscount,int> 
    {
        Task<OrderDiscount> GetByCodeAsync(string code);
        Task<ProductDiscount?> GetActiveDiscountByProductIdAsync(int productId, CancellationToken cancellationToken);

    }

}
