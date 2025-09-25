using ErrorOr;
using MediatR;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record DeleteOrderItemCommand(int OrderItemId, int UserId) : IRequest<ErrorOr<Success>>;
    public class DeleteOrderItemCommandHandler: IRequestHandler<DeleteOrderItemCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderItemCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteOrderItemCommand request, CancellationToken cancellationToken)
        {
            return await _orderRepository.DeleteOrderItemAsync(request.OrderItemId, request.UserId);
        }
    }

}
