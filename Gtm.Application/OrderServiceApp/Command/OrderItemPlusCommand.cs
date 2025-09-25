using ErrorOr;
using MediatR;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record OrderItemPlusCommand(int OrderItemId, int UserId) : IRequest<ErrorOr<Success>>;
    public class OrderItemPlusCommandHandler : IRequestHandler<OrderItemPlusCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderItemPlusCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(OrderItemPlusCommand request, CancellationToken cancellationToken)
        {
            return await _orderRepository.OrderItemPlus(request.OrderItemId, request.UserId);
        }
    }
}