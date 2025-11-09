using ErrorOr;
using MediatR;

namespace Gtm.Application.OrderServiceApp.Command
{
     
     public record CancelOrderSellersCommand(int Id) : IRequest<ErrorOr<Success>>;
     public class CancelOrderSellersCommandHandler
   : IRequestHandler<CancelOrderSellersCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public CancelOrderSellersCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(
            CancelOrderSellersCommand request,
            CancellationToken cancellationToken)
        {
            // اجرای لغو فروشندگان سفارش
            bool result = await _orderRepository.CancelOrderSellersAsync(request.Id);

            if (!result)
                return Error.Failure("OrderSeller.CancelFailed", "لغو فروشندگان این سفارش انجام نشد یا سفارش یافت نشد.");

            return Result.Success;
        }
    }

}
