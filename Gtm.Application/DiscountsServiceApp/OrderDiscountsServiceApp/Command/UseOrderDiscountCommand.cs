using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
    public record UseOrderDiscountCommand(int id, int count):IRequest<ErrorOr<Success>>;
    public class UseOrderDiscountCommandHandler : IRequestHandler<UseOrderDiscountCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(UseOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
