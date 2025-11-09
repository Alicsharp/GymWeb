using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;

namespace Gtm.Application.OrderServiceApp.Query
{
    /// <summary>
    /// کوئری برای دریافت لیست فروش‌های فروشنده به صورت صفحه‌بندی شده
    /// </summary>
    public record GetOrderSellersForSellerQuery(int UserId, int PageId, int Take)
        : IRequest<OrderSellerPaging>;
    public class GetOrderSellersForSellerQueryHandler
     : IRequestHandler<GetOrderSellersForSellerQuery, OrderSellerPaging>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISellerRepository _sellerRepository;

        public GetOrderSellersForSellerQueryHandler(
            IOrderRepository orderRepository,
            ISellerRepository sellerRepository)
        {
            _orderRepository = orderRepository;
            _sellerRepository = sellerRepository;
        }

        public async Task<OrderSellerPaging> Handle(GetOrderSellersForSellerQuery request, CancellationToken cancellationToken)
        {
            // 1. دریافت IQueryable پایه از ریپازیتوری (بدون اجرای کوئری)
            var baseQuery = _orderRepository.GetPaidOrderSellersForUserQueryable(request.UserId);

            // 2. ساخت مدل صفحه‌بندی
            OrderSellerPaging model = new OrderSellerPaging();

            // 3. فراخوانی متد GetData (دقیقاً طبق کد شما)
            // ⚠️ هشدار: این متد به صورت همزمان (Sync) کوئری Count() را 
            // چندین بار به دیتابیس می‌زند که باعث مسدود شدن رشته می‌شود.
            model.GetData(baseQuery, request.PageId, request.Take, 2);

            // 4. واکشی داده‌های صفحه فعلی (Async)
            // ما از مقادیر Skip و Take که توسط GetData محاسبه شده، استفاده می‌کنیم
            List<OrderSeller> orderSellers = await baseQuery
                                                    .Skip(model.Skip)
                                                    .Take(model.Take)
                                                    .ToListAsync(cancellationToken);

            // 5. مپ کردن انتیتی‌ها به ViewModel
            model.OrderSellers = orderSellers.Select(o => new OrderSellerQueryModel
            {
                PriceAfterOff = o.PriceAfterOff,
                DiscountId = o.DiscountId,
                DiscountPercent = o.DiscountPercent,
                DiscountPrice = o.DiscountPercent * o.PriceAfterOff / 100,
                DiscountTitle = o.DiscountTitle,
                Id = o.Id,
                OrderId = o.OrderId,
                PaymentPrice = o.PaymentPrice,
                PostId = o.PostId,
                PostPrice = o.PostPrice,
                PostTitle = o.PostTitle,
                Price = o.Price,
                SellerId = o.SellerId,
                Status = o.Status,
                UpdateDate = o.Order.UpdateDate.ToPersainDate(),
                SellerName = "" // (در حلقه N+1 پر می‌شود)
            }).ToList();

            // 6. اجرای حلقه N+1 (حفظ ساختار اصلی اما به صورت async)
            if (model.OrderSellers.Count > 0)
            {
                foreach (var x in model.OrderSellers)
                {
                    var seller = await _sellerRepository.GetByIdAsync(x.SellerId);
                    if (seller != null)
                    {
                        x.SellerName = seller.Title;
                    }
                }
            }

            return model;
        }
    }
   
}

