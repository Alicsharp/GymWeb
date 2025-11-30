using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.UserApp;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.SellerDomain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.AdminDashbord.Query
{
    /// <summary>
    /// کوئری برای دریافت لیست سفارش‌ها برای پنل ادمین با فیلتر و صفحه‌بندی
    /// </summary>
    public record GetOrdersForAdminQuery(int PageId, int Take, int OrderId, int UserId, OrderAdminStatus Status)
        : IRequest<ErrorOr<OrderAdminPaging>>;
    public class GetOrdersForAdminQueryHandler
    : IRequestHandler<GetOrdersForAdminQuery, ErrorOr<OrderAdminPaging>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepo _userRepository;

        public GetOrdersForAdminQueryHandler(IOrderRepository orderRepository, IUserRepo userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public async Task<ErrorOr<OrderAdminPaging>> Handle(
             GetOrdersForAdminQuery request, CancellationToken cancellationToken)
        {
            // 1. دریافت IQueryable پایه از ریپازیتوری (اجرا نشده)
            var baseQuery = _orderRepository.GetOrdersForAdminQueryable(
                request.OrderId, request.UserId, request.Status
            );

            // 2. ساخت مدل صفحه‌بندی
            OrderAdminPaging model = new();

            // 3. فراخوانی متد GetData همزمان (Sync) شما
            // ⚠️ هشدار: این متد رشته را مسدود (block) می‌کند و ناکارآمد است،
            // اما دقیقاً طبق خواسته شما از BasePaging استفاده می‌کند.
            model.GetData(baseQuery, request.PageId, request.Take, 2);

            // 4. واکشی داده‌های صفحه فعلی به صورت Async (با استفاده از Skip و Take محاسبه شده)
            var orders = await baseQuery
                .Skip(model.Skip)
                .Take(model.Take)
                .ToListAsync( );
            //// 4. واکشی داده‌های صفحه فعلی به صورت Async (با استفاده از Skip و Take محاسبه شده)
            //var orders = await baseQuery
            //    .Skip(model.Skip)
            //    .Take(model.Take)
            //    .ToListAsync(cancellationToken);

            // --- حل مشکل N+1 در هندلر ---
            // 5. جمع‌آوری تمام شناسه‌های کاربری
            var userIds = orders.Select(o => o.UserId).Distinct().ToList();

            // 6. واکشی تمام کاربران مورد نیاز در *یک* کوئری
            var users = await _userRepository.GetUsersByIdsAsync(userIds, cancellationToken);
            var userDictionary = users.ToDictionary(u => u.Id);
            // --- پایان حل N+1 ---

            // 7. تنظیم سایر مشخصات مدل
            model.Status = request.Status;
            model.OrderId = request.OrderId;
            model.UserId = request.UserId;
            model.PageTitle = await GeneratePageTitleAsync(request.Status, request.UserId, _userRepository);

            // 8. مپ کردن نهایی
            model.Orders = orders.Select(o => new OrderForAdminQueryModel
            {
                Id = o.Id,
                OrderStatus = o.OrderStatus,
                PaymentPrice = o.PaymentPrice,
                UserId = o.UserId,
                CreationDate = o.CreateDate.ToPersianDate(),
                UpdateDate = o.UpdateDate.ToPersianDate(),
                DiscountId = o.DiscountId,
                DiscountPercent = o.DiscountPercent,
                DiscountTitle = o.DiscountTitle,
                PriceAfterOff = o.PriceAfterOff,
                PaymentPriceSeller = o.PaymentPriceSeller,
                PostPrice = o.PostPrice,
                Price = o.Price,
                UserName = userDictionary.TryGetValue(o.UserId, out var user)
                    ? (string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName)
                    : "کاربر حذف شده",
                BackgroundColor = GetStatusColor(o.OrderStatus)
            }).ToList();

            return model;
        }


        /// <summary>
        /// Generates the dynamic page title based on filters.
        /// </summary>
        private async Task<string> GeneratePageTitleAsync(OrderAdminStatus status, int userId, IUserRepo userRepo)
        {
            string title = "لیست همه فاکتور ها";
            switch (status)
            {
                case OrderAdminStatus.پرداخت_نشده:
                    title = "لیست فاکتور های پرداخت نشده";
                    break;
                case OrderAdminStatus.پرداخت_شده:
                    title = "لیست فاکتور های پرداخت شده";
                    break;
                case OrderAdminStatus.لغو_شده_توسط_مشتری:
                    title = "لیست فاکتور های لغو شده توسط مشتری";
                    break;
                case OrderAdminStatus.لغو_شده_توسط_ادمین:
                    title = "لیست فاکتور های لغو شده توسط ادمین";
                    break;
                case OrderAdminStatus.ارسال_شده:
                    title = "لیست فاکتور های ارسال شده ";
                    break;
                    // (Default is "لیست همه فاکتور ها")
            }

            if (userId > 0)
            {
                // (We call GetByIdAsync from the generic IRepository)
                var user = await userRepo.GetByIdAsync(userId);
                if (user != null)
                {
                    var userName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                    title = title + $" برای کاربر : {userName}";
                }
            }
            return title;
        }
      
        private string GetStatusColor(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.پرداخت_شده:
                    return "green";
                case OrderStatus.لغو_شده_توسط_مشتری:
                case OrderStatus.لغو_شده_توسط_ادمین:
                    return "red";
                case OrderStatus.ارسال_شده:
                    return "aqua";
                case OrderStatus.پرداخت_نشده:
                default:
                    return "orange";
            }
        }
    }

 }
    

 