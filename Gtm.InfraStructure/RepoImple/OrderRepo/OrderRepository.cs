using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.UserApp;
using Gtm.Contract.OrderAddressContract.Command;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using Gtm.Domain.ShopDomain.ProductSellDomain;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;
using static Gtm.InfraStructure.RepoImple.OrderRepo.OrderRepository;

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
        public async Task<int> CalculateOrderSellerWeightAsync(int orderSellerId)
        {
            var seller = await _context.OrderSellers
                .Include(s => s.OrderItems)
                    .ThenInclude(i => i.ProductSell) // واکشی محصول برای دسترسی به وزن (Weight)
                .SingleOrDefaultAsync(s => s.Id == orderSellerId);

            if (seller == null)
                return 0;

            // (بهینه‌سازی شده از حلقه foreach)
            // محاسبه مجموع وزن = (تعداد هر آیتم * وزن واحد آن آیتم)
            int totalWeight = seller.OrderItems
                .Sum(item => item.Count * item.ProductSell.Weight);

            return totalWeight;
        }
        /// <summary>
        /// یک آدرس سفارش را بر اساس شناسه آن واکشی می‌کند.
        /// </summary>
        public async Task<OrderAddress> GetOrderAddressByIdAsync(int id)
        {
            // FindAsync بهترین راه برای واکشی بر اساس کلید اصلی (PK) است
            return await _context.OrderAddresses.SingleOrDefaultAsync(o=>o.Id==id);
        }

        /// <summary>
        /// یک آدرس سفارش جدید را در دیتابیس ایجاد کرده و شناسه (کلید) آن را بازمی‌گرداند.
        /// </summary>
        public async Task<int> CreateOrderaddressReturnKey(OrderAddress orderAddress)
        {
            // 1. آدرس را به DbContext اضافه کن
            await _context.OrderAddresses.AddAsync(orderAddress);

            // 2. تغییرات را ذخیره کن تا دیتابیس شناسه را تخصیص دهد
            // (این متد برخلاف SaveAsync که bool برمی‌گرداند، باید ذخیره کند
            // چون ما به ID نیاز داریم)
            try
            {
                await _context.SaveChangesAsync();

                // 3. شناسه ایجاد شده را برگردان
                return orderAddress.Id;
            }
            catch (DbUpdateException ex)
            {
                // لاگ کردن خطا (ex)
                return 0; // 0 به معنی شکست در ایجاد است
            }
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
                await SaveChangesAsync();
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
            // 1. واکشی بهینه: فقط آیتم مورد نظر را پیدا می‌کنیم
            var orderItem = await _context.OrderItems
                .Include(oi => oi.OrderSeller) // (برای آپدیت قیمت پست)
                    .ThenInclude(os => os.Order)
                .SingleOrDefaultAsync(oi =>
                   
                    oi.OrderSeller.Order.UserId == userId &&
                    oi.OrderSeller.Order.OrderStatus == OrderStatus.پرداخت_نشده);;

            // 2. بررسی آیا آیتم یافت شد؟
            if (orderItem == null)
            {
                return Error.NotFound(description: "آیتم مورد نظر در سبد خرید یافت نشد.");
            }

            // 3. گرفتن فروشنده قبل از حذف
            var seller = orderItem.OrderSeller;

            // 4. حذف آیتم از Context (در حافظه)
            _context.OrderItems.Remove(orderItem);
          await  SaveChangesAsync();

            // 5. اعمال منطق بیزینس (ریست کردن قیمت پست)
            if (seller != null)
            {
                seller.AddPostPrice(0, 0, "");
                _context.OrderSellers.Update(seller); // (علامت‌گذاری برای آپدیت)
            }

            // 6. موفقیت (آماده برای ذخیره توسط هندلر)
            return Result.Success;
        }

        public async Task<int> GetActiveOrderSellerCountAsync(CancellationToken cancellationToken = default)
        {
            // (این کد از متد اصلی شما گرفته شده است)
            return await _context.OrderSellers.CountAsync(o =>
                o.Status ==  OrderSellerStatus.پرداخت_شده ||
                o.Status == OrderSellerStatus.در_حال_آماده_سازی ||
                o.Status == OrderSellerStatus.ارسال_شده,
                cancellationToken);
        }

         
        public async Task<int> GetActiveOrderItemCountAsync(CancellationToken cancellationToken = default)
        {
            // (این کد از متد اصلی شما گرفته شده است)
            return await _context.OrderItems
                .Include(i => i.OrderSeller) // (Include برای اعمال شرط Where ضروری است)
                .CountAsync(i =>
                    i.OrderSeller.Status == OrderSellerStatus.پرداخت_شده ||
                    i.OrderSeller.Status == OrderSellerStatus.در_حال_آماده_سازی ||
                    i.OrderSeller.Status == OrderSellerStatus.ارسال_شده,
                cancellationToken);
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
        public async Task<bool> HaveUserOpenOrderAsync(int userId)
        {
            // (فرض می‌کنیم _context همان DbContext شماست
            // و OrderStatus.Open وضعیت سبد باز را مشخص می‌کند)

            return await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.OrderStatus == OrderStatus.پرداخت_نشده);
        }
        public async Task<Order> GetOpenOrderWithDetailsAsync(int userId)
        {
            // این کوئری تمام N+1 Query ها را حل می‌کند
            return await _context.Orders
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.Seller) // <- واکشی اطلاعات فروشگاه
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.OrderItems)
                    .ThenInclude(i => i.ProductSell)
                    .ThenInclude(ps => ps.Product) // <- واکشی محصول و محصول-فروشنده
                .SingleOrDefaultAsync(s => s.UserId == userId &&
                                          s.OrderStatus == OrderStatus.پرداخت_نشده);
        }
        public async Task<int> CalculateOrderSellerWeight(int orderSellerId)
        {
            var seller = await _context.OrderSellers
                .Include(s => s.OrderItems)
                    .ThenInclude(i => i.ProductSell)
                .SingleOrDefaultAsync(s => s.Id == orderSellerId);

            if (seller == null)
                return 0;

            // This loop can be simplified to a LINQ Sum()
            int weight = seller.OrderItems.Sum(item => item.Count * item.ProductSell.Weight);

            return weight;
        }

        public async Task<Order> GetOpenOrderWithItemsAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderSellers)
                    .ThenInclude(o => o.OrderItems)
                .SingleOrDefaultAsync(o => o.UserId == userId && o.OrderStatus == OrderStatus.پرداخت_نشده);
        }
        public async Task<Order> GetPaidOrderDetailAsync(int orderId)
        {
            return await _context.Orders
                // 1. واکشی آدرس سفارش
                .Include(o => o.OrderSellers)
                // 2. واکشی فروشندگان و اطلاعات خود فروشنده (Seller)
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.Seller)
                // 3. واکشی آیتم‌ها، محصول-فروشنده و خود محصول
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.OrderItems)
                    .ThenInclude(i => i.ProductSell)
                    .ThenInclude(ps => ps.Product)
                // 4. فیلتر نهایی
                .SingleOrDefaultAsync(o => o.Id == orderId &&
                                         o.OrderStatus == OrderStatus.پرداخت_شده);
        }

        public async Task<Order> GetOrderDetailForUserPanelAsync(int id, int userId)
        {

            var order = await _context.Orders.Include(o => o.OrderSellers).ThenInclude(s => s.OrderItems)
                .SingleOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            return order;
        }

        public IQueryable<Order> GetUserOrdersQueryable(int userId)
        {// این دقیقاً همان کدی است که شما ارائه کردید
            var query = _context.Orders
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id);

            return query;
        }
        public async Task<Order> GetOrderWithSellersAndItemsAsync(int orderId)
        {
            // این همان کوئری اول کد اصلی شماست
            return await _context.Orders
                .Include(o => o.OrderSellers)
                    .ThenInclude(s => s.OrderItems)
                .SingleOrDefaultAsync(o => o.Id == orderId);
        }

        public IQueryable<OrderSeller> GetPaidOrderSellersForUserQueryable(int userId)
        {
            // بهینه‌سازی شده:
            // به جای ToList() و سپس Contains()، ما از یک زیرکوئری Any() استفاده می‌کنیم
            // که در SQL به 'WHERE EXISTS' یا 'WHERE IN (SELECT ...)' ترجمه می‌شود.

            var query = _context.OrderSellers
                .Include(o => o.Order)
                .Include(o => o.OrderItems)
                    .ThenInclude(I => I.ProductSell)
                .Where(o => o.Status !=  OrderSellerStatus.پرداخت_نشده &&
                            // این بخش جایگزین دو کوئری اول شما شده است:
                            _context.Sellers.Any(s => s.Id == o.SellerId && s.UserId == userId))
                .OrderByDescending(o => o.Id);

            return query;
        }
        
        public async Task<OrderSeller> GetOrderSellerWithDetailsAsync(int orderSellerId, CancellationToken cancellationToken = default)
        {
                return await _context.OrderSellers
                    .Include(o => o.Order) // برای دسترسی به UserId و AddressId
                    .Include(o => o.OrderItems)
                        .ThenInclude(I => I.ProductSell) // برای آیتم‌ها
                    .SingleOrDefaultAsync(s => s.Id == orderSellerId, cancellationToken);
        }
        public async Task<bool> ChangeOrderSellerStatusBySellerAsync(int orderSellerId, OrderSellerStatus status, int userId)
        {
            // 1. کوئری پیچیده شما باید باقی بماند (این درست است)
            // (ما از _context که در BaseRepository است استفاده می‌کنیم)
            var orderSeller = await _context.OrderSellers
                .Include(s => s.Order)
                .Include(s => s.Seller)
                .SingleOrDefaultAsync(s => s.Id == orderSellerId);

            // 2. منطق بررسی امنیتی
            if (orderSeller == null || orderSeller.Seller.UserId != userId)
                return false;

            // 3. منطق تجاری (State Machine)
            switch (orderSeller.Status)
            {
                case OrderSellerStatus.پرداخت_شده:
                    if (status == OrderSellerStatus.لغو_شده_توسط_فروشنده || status == OrderSellerStatus.در_حال_آماده_سازی)
                    {
                        orderSeller.ChangeStatus(status);

                        // 4. اصلاح نهایی: استفاده از نام متد جدید IRepository
                        return await SaveChangesAsync(); // (به جای SaveAsync)
                    }
                    break;

                case OrderSellerStatus.در_حال_آماده_سازی:
                    if (status == OrderSellerStatus.لغو_شده_توسط_فروشنده || status == OrderSellerStatus.ارسال_شده)
                    {
                        orderSeller.ChangeStatus(status);

                        // 4. اصلاح نهایی: استفاده از نام متد جدید IRepository
                        return await SaveChangesAsync(); // (به جای SaveAsync)
                    }
                    break;
            }

            return false;
        }
        public async Task<List<Order>> GetLast10OrdersAsync(CancellationToken cancellationToken = default)
        {
            // توجه: Include ها برای این Select خاص مورد نیاز نیستند
            // چون فقط از فیلدهای خود Order استفاده می‌شود.
            return await _context.Orders
                .OrderByDescending(o => o.UpdateDate)
                .Take(10)
                .ToListAsync(cancellationToken);
        }
        public IQueryable<Order> GetOrdersForAdminQueryable(int orderId, int userId, OrderAdminStatus status)
        {
            // 1. کوئری پایه (بدون Include<User> چون وجود ندارد)
            var query = _context.Orders
                .OrderByDescending(o => o.Id)
                .AsQueryable(); // (تبدیل به IQueryable برای اعمال فیلتر)

            // 2. اعمال فیلترهای داینامیک
            if (orderId > 0)
                query = query.Where(r => r.Id == orderId);
            if (userId > 0)
                query = query.Where(r => r.UserId == userId);

            switch (status)
            {
                case OrderAdminStatus.پرداخت_نشده:
                    query = query.Where(r => r.OrderStatus == OrderStatus.پرداخت_نشده);
                    break;
                case OrderAdminStatus.پرداخت_شده:
                    query = query.Where(r => r.OrderStatus == OrderStatus.پرداخت_شده);
                    break;
                    // ... (سایر وضعیت‌ها)
            }

            return query; // 3. کوئری (اجرا نشده) را برمی‌گرداند
        }
        public async Task<Order?> GetOrderDetailForAdminAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Orders
            .Include(o => o.OrderSellers)
                .ThenInclude(s => s.Seller)             // 👈 اضافه شد
            .Include(o => o.OrderSellers)
                .ThenInclude(s => s.OrderItems)
                    .ThenInclude(i => i.ProductSell)
                        .ThenInclude(p => p.Product)   // 👈 اضافه شد
            .Include(o => o.OrderAddress)              // 👈 قبلا اضافه شده
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        }
        public async Task<bool> CancelByAdminAsync(int id)
        {
            var order = await _context.Orders
                .SingleOrDefaultAsync(s => s.Id == id && s.OrderStatus == OrderStatus.پرداخت_شده);
            if (order == null) return false;
            order.ChamgeStatus(OrderStatus.لغو_شده_توسط_ادمین);
            return await SaveChangesAsync();
        }

        public async Task<bool> CancelOrderSellersAsync(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderSellers)
                 .SingleOrDefaultAsync(s => s.Id == id && s.OrderStatus == OrderStatus.پرداخت_شده);
            if (order == null || order.OrderStatus != OrderStatus.پرداخت_شده) return false;
            foreach (var item in order.OrderSellers)
            {
                if (item.Status != OrderSellerStatus.لغو_شده_توسط_فروشنده)
                    item.ChangeStatus(OrderSellerStatus.لغو_شده_توسط_ادمین);
            }
            return await SaveChangesAsync();
        }
        public async Task<OrderSeller?> GetOrderSellerDetailForSellerPanelAsync(int orderSellerId, CancellationToken cancellationToken)
        {
            return await _context.OrderSellers
                .Include(os => os.Order)
                .Include(os => os.OrderItems)
                    .ThenInclude(oi => oi.ProductSell)
                .SingleOrDefaultAsync(os => os.Id == orderSellerId, cancellationToken);
        }
        public async Task<OrderDetailAddressUserPanelQueryModel?> GetOrderAddressWithCityAndStateAsync(int orderAddressId)
        {
            var address = await _context.OrderAddresses
                .AsNoTracking()
                .SingleOrDefaultAsync(o => o.Id == orderAddressId);

            if (address == null)
                return null;

            var city = await _context.Cities
                .Include(c => c.State)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == address.CityId && c.StateId == address.StateId);

            if (city == null)
                return null;

            return new OrderDetailAddressUserPanelQueryModel
            {
                AddressDetail = address.AddressDetail,
                City = city.Title,
                CityId = address.CityId,
                FullName = address.FullName,
                IranCode = address.IranCode,
                Phone = address.Phone,
                PostalCode = address.PostalCode,
                State = city.State.Title,
                StateId = address.StateId
            };
        }


    }
}
