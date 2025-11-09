using ErrorOr;
using MediatR;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record ImperfectOrderCommand(int OrderId) : IRequest<ErrorOr<Success>>;

    public class ImperfectOrderCommandHandler : IRequestHandler<ImperfectOrderCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public ImperfectOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(ImperfectOrderCommand request, CancellationToken cancellationToken)
        {
            // واکشی سفارش
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                return Error.NotFound("Order.NotFound", "سفارشی با این شناسه یافت نشد.");

            // بررسی وضعیت پرداخت
            if (order.OrderStatus != OrderStatus.پرداخت_شده)
                return Error.Validation("Order.StatusInvalid", "وضعیت سفارش برای ناقص کردن نامعتبر است.");

            // تغییر وضعیت به ناقص
            order.ChamgeStatus(OrderStatus.ناقص);

            // ذخیره تغییرات
            bool result = await _orderRepository.SaveChangesAsync(cancellationToken);
            if (!result)
                return Error.Failure("Order.SaveFailed", "ذخیره تغییرات سفارش موفقیت‌آمیز نبود.");

            return Result.Success;
        }
    }

}