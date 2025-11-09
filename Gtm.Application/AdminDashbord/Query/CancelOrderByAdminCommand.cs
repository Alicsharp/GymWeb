using ErrorOr;
using Gtm.Application.OrderServiceApp;
using MediatR;

namespace Gtm.Application.AdminDashbord.Query
{
    public record CancelOrderByAdminCommand(int Id) : IRequest<ErrorOr<Success>>;
    public class CancelOrderByAdminCommandHandler
    : IRequestHandler<CancelOrderByAdminCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public CancelOrderByAdminCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(
            CancelOrderByAdminCommand request,
            CancellationToken cancellationToken)
        {
            // اجرای عملیات لغو سفارش در ریپازیتوری
            bool result = await _orderRepository.CancelByAdminAsync(request.Id);

            if (!result)
                return Error.Failure("Order.CancelFailed", "لغو سفارش با خطا مواجه شد یا سفارشی با این شناسه وجود ندارد.");

            return Result.Success;
        }
    }

}
