using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record CheckOrderEmptyCommand(int UserId) : IRequest<ErrorOr<Success>>;

    public class CheckOrderEmptyCommandHandler: IRequestHandler<CheckOrderEmptyCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public CheckOrderEmptyCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CheckOrderEmptyCommand request, CancellationToken cancellationToken)
        {
            return await _orderRepository.CheckOrderEmpty(request.UserId);
        }
    }
}
