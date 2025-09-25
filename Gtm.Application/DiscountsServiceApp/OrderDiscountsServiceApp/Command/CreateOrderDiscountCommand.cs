using ErrorOr;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
    public record CreateOrderDiscountCommand(CreateOrderDiscount command, OrderDiscountType type, int shopId):IRequest<ErrorOr<Success>>;
    public class CreateOrderDiscountCommandHandler : IRequestHandler<CreateOrderDiscountCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(CreateOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
