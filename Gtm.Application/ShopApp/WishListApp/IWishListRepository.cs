using ErrorOr;
using Gtm.Domain.ShopDomain.WishListAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.WishListApp
{
    public interface IWishListRepository : IRepository<WishList, int>
    {
        Task<List<WishList>> GetLast4WishListItemsAsync(int userId);
        Task<ErrorOr<Success>> DeleteUserProductWishListAsync(int userId, int id);
    }

}
