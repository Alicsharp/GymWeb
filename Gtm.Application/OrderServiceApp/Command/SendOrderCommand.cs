using ErrorOr;
using MediatR;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record SendOrderCommand(int OrderId) : IRequest<ErrorOr<Success>>;
    public class SendOrderCommandHandler : IRequestHandler<SendOrderCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public SendOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(SendOrderCommand request, CancellationToken cancellationToken)
        {
            // واکشی سفارش
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                return Error.NotFound("Order.NotFound", "سفارشی با این شناسه یافت نشد.");

            // بررسی وضعیت سفارش
            if (order.OrderStatus != OrderStatus.پرداخت_شده)
                return Error.Validation("Order.StatusInvalid", "وضعیت سفارش برای ارسال نامعتبر است.");

            // تغییر وضعیت به ارسال شده
            order.ChamgeStatus(OrderStatus.ارسال_شده);

            // ذخیره تغییرات
            bool result = await _orderRepository.SaveChangesAsync(cancellationToken);
            if (!result)
                return Error.Failure("Order.SaveFailed", "ذخیره تغییرات سفارش موفقیت‌آمیز نبود.");

            return Result.Success;
        }
    }

}