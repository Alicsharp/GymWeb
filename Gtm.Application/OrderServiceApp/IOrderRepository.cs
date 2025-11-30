using ErrorOr;
using Gtm.Contract.OrderAddressContract.Command;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp
{
    public interface IOrderRepository:IRepository<Order,int>
    {
         Task<Order> GetOpenOrderForUserAsync(int userId, CancellationToken cancellationToken);
        Task<int> CalculateOrderSellerWeightAsync(int orderSellerId);
        //Task<OrderUserPanelQueryModel> GetOpenOrderForUserAsync(int userId);
        Task<int> CalculateOrderSellerWeight(int orderSellerId);
        Task<Order> GetUnpaidOrderWithItemsAsync(int userId, CancellationToken cancellationToken);
        Task<ErrorOr<Success>> CheckOrderEmpty(int userId);
        Task<ErrorOr<Success>> DeleteOrderItemAsync(int id, int userId);
        Task<bool> HaveUserOpenOrderAsync(int userId);
        Task<ErrorOr<int>> HaveUserOpenOrderSellerAsyncByOrderSellerIdAsync(int userId, int id);
        Task<ErrorOr<Success>> OrderItemMinus(int id, int userId);
        Task<ErrorOr<Success>> OrderItemPlus(int id, int userId);
        Task<Order> GetOpenOrderWithDetailsAsync(int userId);
        /// <summary>
        /// یک آدرس سفارش را بر اساس شناسه آن واکشی می‌کند.
        /// </summary>
        Task<OrderAddress> GetOrderAddressByIdAsync(int id);

        /// <summary>
        /// یک آدرس سفارش جدید را در دیتابیس ایجاد کرده و شناسه (کلید) آن را بازمی‌گرداند.
        /// </summary>
        Task<int> CreateOrderaddressReturnKey(OrderAddress orderAddress);
        Task<Order> GetOpenOrderWithItemsAsync(int userId);
        Task<Order> GetPaidOrderDetailAsync(int orderId);
        Task<Order> GetOrderDetailForUserPanelAsync(int id, int userId);
 
        /// <summary>
        /// یک کوئری IQueryable برای سفارش‌های کاربر (شامل جزئیات کامل) 
        /// که بر اساس شناسه به صورت نزولی مرتب شده‌اند، برمی‌گرداند.
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>IQueryable<Order></returns>
        IQueryable<Order> GetUserOrdersQueryable(int userId);
        Task<Order> GetOrderWithSellersAndItemsAsync(int orderId);
        /// <summary>
        /// یک کوئری IQueryable برای فروشندگان-سفارش پرداخت شده 
        /// مرتبط با یک کاربر (مالک فروشگاه) برمی‌گرداند.
        /// </summary>
        IQueryable<OrderSeller> GetPaidOrderSellersForUserQueryable(int userId);

        /// <summary>
        /// یک فروشنده-سفارش را به همراه سفارش، آیتم‌ها، و محصول-فروشنده واکشی می‌کند.
        /// </summary>
        Task<OrderSeller> GetOrderSellerWithDetailsAsync(int orderSellerId, CancellationToken cancellationToken = default);
        Task<bool> ChangeOrderSellerStatusBySellerAsync(int orderSellerId, OrderSellerStatus status, int userId);

        /// <summary>
        /// تعداد کل OrderSeller های فعال (پرداخت شده، در حال آماده‌سازی، ارسال شده) را برمی‌گرداند
        /// </summary>
        Task<int> GetActiveOrderSellerCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// تعداد کل OrderItem های درون OrderSeller های فعال را برمی‌گرداند
        /// </summary>
        Task<int> GetActiveOrderItemCountAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// ۱۰ سفارش آخر (بر اساس تاریخ آپدیت) را واکشی می‌کند.
        /// </summary>
        Task<List<Order>> GetLast10OrdersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// سفارش‌های ادمین را با فیلترهای داینامیک و به صورت صفحه‌بندی شده واکشی می‌کند.
        /// (این متد User را Include می‌کند تا مشکل N+1 حل شود)
        /// </summary>
        public IQueryable<Order> GetOrdersForAdminQueryable(int orderId, int userId, OrderAdminStatus status);

        Task<Order?> GetOrderDetailForAdminAsync(int id, CancellationToken cancellationToken);
        Task<bool> CancelByAdminAsync(int id);

        Task<bool> CancelOrderSellersAsync(int id);
        Task<OrderSeller?> GetOrderSellerDetailForSellerPanelAsync(int orderSellerId, CancellationToken cancellationToken);
        Task<OrderDetailAddressUserPanelQueryModel?> GetOrderAddressWithCityAndStateAsync(int orderAddressId);

    }

}

