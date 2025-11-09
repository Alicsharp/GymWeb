using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record PaymentSuccessOrderCommand(int UserId, int Price) : IRequest<ErrorOr<int>>;
    public class PaymentSuccessOrderCommandHandler: IRequestHandler<PaymentSuccessOrderCommand, ErrorOr<int>>
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentSuccessOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<int>> Handle(PaymentSuccessOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. واکشی سفارش باز
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.UserId);

            // 2. اعتبارسنجی (این بخش حیاتی در کد اصلی شما وجود نداشت)
            if (order == null)
            {
                return Error.NotFound(description: "سبد خرید بازی یافت نشد.");
            }

            // 3. اعتبارسنجی قیمت
            if (order.PaymentPrice != request.Price)
            {
                // به جای: return 0;
                return Error.Validation(description: "مبلغ پرداخت شده با مبلغ فاکتور مطابقت ندارد.");
            }

            // 4. اعمال تغییرات (تغییر وضعیت)
            order.ChamgeStatus(OrderStatus.پرداخت_شده);
            foreach (var item in order.OrderSellers)
            {
                item.ChangeStatus(OrderSellerStatus.پرداخت_شده);
            }

            // 5. ذخیره تغییرات
            if (await _orderRepository.SaveChangesAsync())
            {
                // به جای: return order.Id;
                return order.Id; // بازگشت موفقیت‌آمیز شناسه سفارش
            }

            // 6. مدیریت خطای ذخیره‌سازی
            // به جای: return 0;
            return Error.Failure(description: "خطا در ذخیره‌سازی سفارش پرداخت شده.");
        }
    }
}
