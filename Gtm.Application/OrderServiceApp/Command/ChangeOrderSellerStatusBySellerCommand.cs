using ErrorOr;
using MediatR;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp.Command
{
    /// <summary>
    /// کامند برای تغییر وضعیت یک فروش-سفارش توسط فروشنده (با بررسی امنیتی)
    /// </summary>
    public record ChangeOrderSellerStatusBySellerCommand(
        int OrderSellerId,
        OrderSellerStatus Status,
        int UserId) : IRequest<ErrorOr<Success>>;

    public class ChangeOrderSellerStatusBySellerCommandHandler
    : IRequestHandler<ChangeOrderSellerStatusBySellerCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;
        // (بر اساس کد شما، این متد در IOrderRepository است)

        public ChangeOrderSellerStatusBySellerCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(ChangeOrderSellerStatusBySellerCommand request, CancellationToken cancellationToken)
        {
            // فراخوانی مستقیم متد ریپازیتوری، همانطور که در کد شما بود
            // (من املای Chnage را برای تطابق با کد شما حفظ کرده‌ام)
            bool success = await _orderRepository.ChangeOrderSellerStatusBySellerAsync(
                request.OrderSellerId,
                request.Status,
                request.UserId
            );

            if (success)
            {
                return Result.Success;
            }

            // اگر ریپازیتوری false برگرداند (مثلاً به دلیل شکست در ذخیره یا عدم دسترسی کاربر)
            return Error.Failure(description: "خطا در تغییر وضعیت سفارش.");
            // (یا می‌توانید از Error.Validation یا Error.Forbidden استفاده کنید
            // اگر ریپازیتوری شما اطلاعات دقیق‌تری برمی‌گرداند)
        }
    }
}
