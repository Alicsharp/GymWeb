using ErrorOr;
using MediatR;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record ChangeOrderPaymentCommand(int UserId, OrderPayment Payment): IRequest<ErrorOr<Success>>;
    public class ChangeOrderPaymentCommandHandler: IRequestHandler<ChangeOrderPaymentCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;
        // (فرض می‌کنیم ValidationMessages یک کلاس static با پیام‌های خطا است)

        public ChangeOrderPaymentCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(ChangeOrderPaymentCommand request, CancellationToken cancellationToken)
        {
            // 1. واکشی سفارش باز
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.UserId);

            // 2. اعتبارسنجی (این بخش حیاتی در کد اصلی شما وجود نداشت)
            if (order == null)
            {
                return Error.NotFound(description: "سبد خرید بازی یافت نشد.");
            }

            // 3. اعمال تغییر در انتیتی
            order.ChangePayment(request.Payment);

            // 4. ذخیره تغییرات
            if (await _orderRepository.SaveChangesAsync())
            {
                // به جای: return new(true);
                return Result.Success;
            }

            // 5. مدیریت خطای ذخیره‌سازی
            // به جای: return new(false, ValidationMessages.SystemErrorMessage);
            return Error.Failure(description: ValidationMessages.SystemErrorMessage);
        }
    }
}
