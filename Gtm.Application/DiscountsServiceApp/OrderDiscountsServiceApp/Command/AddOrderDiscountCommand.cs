using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
    public record AddOrderDiscountCommand(int userId, int id, string title, int percent):IRequest<ErrorOr<Success>>;
    public class AddOrderDiscountCommandHandler : IRequestHandler<AddOrderDiscountCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;
        public async Task<ErrorOr<Success>> Handle(AddOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.userId);
            order.AddDiscount(request.id, request.percent, request.title);
            var res= await _orderRepository.SaveChangesAsync(cancellationToken);
            if(res)
                return Error.Failure();
            return Result.Success;
        }
    }
}
