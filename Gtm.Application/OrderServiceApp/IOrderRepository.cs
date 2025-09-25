using ErrorOr;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.OrderServiceApp
{
    public interface IOrderRepository:IRepository<Order,int>
    {
        Task<Order> GetOpenOrderForUserAsync(int userId);

        //Task<OrderUserPanelQueryModel> GetOpenOrderForUserAsync(int userId);

        Task<Order> GetUnpaidOrderWithItemsAsync(int userId, CancellationToken cancellationToken);
        Task<ErrorOr<Success>> CheckOrderEmpty(int userId);
        Task<ErrorOr<Success>> DeleteOrderItemAsync(int id, int userId);

        Task<ErrorOr<int>> HaveUserOpenOrderSellerAsyncByOrderSellerIdAsync(int userId, int id);
        Task<ErrorOr<Success>> OrderItemMinus(int id, int userId);
        Task<ErrorOr<Success>> OrderItemPlus(int id, int userId);
    }
}
