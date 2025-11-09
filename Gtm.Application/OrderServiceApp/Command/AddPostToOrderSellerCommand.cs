using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record AddPostToOrderSellerCommand(
      int UserId,
      int OrderSellerId,
      int PostId,
      int Price,
      string Title) : IRequest<ErrorOr<Success>>;
    public class AddPostToOrderSellerCommandHandler: IRequestHandler<AddPostToOrderSellerCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public AddPostToOrderSellerCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(AddPostToOrderSellerCommand request, CancellationToken cancellationToken)
        {
            // 1. واکشی سفارش باز کاربر
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.UserId);

            // 2. بررسی حیاتی (رفع باگ NullReferenceException)
            if (order == null)
            {
                return Error.NotFound(description: "سبد خرید بازی یافت نشد.");
            }

            // 3. پیدا کردن فروشنده-سفارش مورد نظر
            var seller = order.OrderSellers.SingleOrDefault(s => s.Id == request.OrderSellerId);

            if (seller == null)
            {
                // به جای: return false;
                return Error.NotFound(description: $"فروشنده-سفارش با شناسه {request.OrderSellerId} در سبد خرید شما یافت نشد.");
            }

            // 4. اعمال تغییرات
            seller.AddPostPrice(request.Price, request.PostId, request.Title);

            // 5. ذخیره تغییرات
            if (await _orderRepository.SaveChangesAsync())
            {
                return Result.Success;
            }

            return Error.Failure(description: "خطا در ذخیره‌سازی اطلاعات پستی.");
        }
    }
}
