using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Domain.ShopDomain.OrderDomain;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.OrderRepo
{
    public class OrderRepository : Repository<Order, int>, IOrderRepository
    {
        private GtmDbContext _context;

        public OrderRepository(GtmDbContext context) :base(context) 
        {
            _context = context;
        }
        public async Task<ErrorOr<Success>> OrderItemPlus(int id, int userId)
        {
            var item = await _context.OrderItems
                .Include(i => i.OrderSeller)
                .ThenInclude(s => s.Order)
                .SingleOrDefaultAsync(o => o.Id == id && o.OrderSeller.Order.UserId == userId);

            if (item is null)
                return Error.NotFound(description: "آیتم سفارش یافت نشد.");

            var productSell = await _context.ProductSells.FindAsync(item.ProductSellId);
            if (productSell is null)
                return Error.NotFound(description: "کالای مربوطه یافت نشد.");

            if (productSell.Amount <= item.Count)
                return Error.Validation(description: "موجودی کافی نیست.");

            item.PlusCount(1);
            await SaveChangesAsync();
            return Result.Success;
        }

        public async Task<ErrorOr<Success>> OrderItemMinus(int id, int userId)
        {
            var item = await _context.OrderItems
                .Include(i => i.OrderSeller)
                .ThenInclude(s => s.Order)
                .SingleOrDefaultAsync(o => o.Id == id && o.OrderSeller.Order.UserId == userId);

            if (item is null)
                return Error.NotFound(description: "آیتم سفارش یافت نشد.");

            if (item.Count == 1)
            {
                var deleted = await DeleteOrderItemAsync(id, userId);
                return deleted;
            }

            item.MinusCount(1);
            await SaveChangesAsync();
            return Result.Success;
        }

        public async Task<Order> GetOpenOrderForUserAsync(int userId)
        {
            var order =
   await _context.Orders.Include(o => o.OrderSellers).ThenInclude(s => s.OrderItems).SingleOrDefaultAsync(s => s.UserId == userId &&
            s.OrderStatus == OrderStatus.پرداخت_نشده);
    
            if (order == null)
            {
                order = new(userId);
                await AddAsync(order);
                return await _context.Orders.Include(o => o.OrderSellers).ThenInclude(o => o.OrderItems)
                .SingleOrDefaultAsync(o => o.UserId == userId && o.OrderStatus == OrderStatus.پرداخت_نشده);
            }
            else
                return order;
        }
        public async Task<Order> GetUnpaidOrderWithItemsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _context.Orders
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.OrderItems)
                .SingleOrDefaultAsync(o =>
                    o.UserId == userId &&
                    o.OrderStatus ==OrderStatus.پرداخت_نشده,
                    cancellationToken);
        }

        public async Task<ErrorOr<Success>> CheckOrderEmpty(int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderSellers)
                .ThenInclude(o => o.OrderItems)
                .SingleOrDefaultAsync(o => o.UserId == userId && o.OrderStatus == OrderStatus.پرداخت_نشده);

            if (order is null)
                return Result.Success; // یعنی چیزی برای حذف وجود نداشت، مشکلی هم نیست

            if (order.OrderSellers.Count == 0)
            {
                _context.Orders.Remove(order);
            }
            else
            {
                foreach (var seller in order.OrderSellers.ToList())
                {
                    if (seller.OrderItems.Count == 0)
                        _context.OrderSellers.Remove(seller);
                }

                if (order.OrderSellers.Count == 0)
                    _context.Orders.Remove(order);
            }

            await SaveChangesAsync();
            return Result.Success;
        }
        public async Task<ErrorOr<Success>> DeleteOrderItemAsync(int id, int userId)
        {
            var item = await _context.OrderItems
                .Include(i => i.OrderSeller)
                .ThenInclude(s => s.Order)
                .SingleOrDefaultAsync(o => o.Id == id && o.OrderSeller.Order.UserId == userId);

            if (item is null)
                return Error.NotFound(description: "آیتم سفارش یافت نشد.");

            if (item.OrderSeller.Order.OrderStatus != OrderStatus.پرداخت_نشده)
                return Error.Validation(description: "این سفارش قابل ویرایش نیست.");

            _context.OrderItems.Remove(item);
            await SaveChangesAsync();

            return Result.Success;
        }

        public async Task<ErrorOr<int>> HaveUserOpenOrderSellerAsyncByOrderSellerIdAsync(int userId, int orderSellerId)
        {
            // گرفتن سفارش باز برای کاربر
            var order = await _context.Orders
                .Include(o => o.OrderSellers)
                .ThenInclude(s => s.OrderItems)
                .SingleOrDefaultAsync(o => o.UserId == userId && o.OrderStatus == OrderStatus.پرداخت_نشده);

            if (order is null)
                return Error.NotFound(description: "سفارشی یافت نشد.");

            // بررسی اینکه OrderSeller با id داده شده وجود دارد یا خیر
            var seller = order.OrderSellers.SingleOrDefault(s => s.SellerId == orderSellerId);
            if (seller is null)
                return Error.NotFound(description: "فروشگاهی یافت نشد.");

            return seller.Id; // مقدار موفق: Id واقعی OrderSeller
        }

    }
}
