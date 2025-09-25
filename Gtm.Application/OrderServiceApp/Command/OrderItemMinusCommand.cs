using ErrorOr;
using MediatR;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record OrderItemMinusCommand(int OrderItemId, int UserId) : IRequest<ErrorOr<Success>>;
    public class OrderItemMinusCommandHandler: IRequestHandler<OrderItemMinusCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderItemMinusCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(OrderItemMinusCommand request, CancellationToken cancellationToken)
        {
            return await _orderRepository.OrderItemMinus(request.OrderItemId, request.UserId);
        }
    }      
}
