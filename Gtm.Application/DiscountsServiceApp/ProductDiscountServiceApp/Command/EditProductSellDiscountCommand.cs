using ErrorOr;
using Gtm.Contract.DiscountsContract.ProductDiscountContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp.Command
{
    public record EditProductSellDiscountCommand(EditProductDiscount command):IRequest<ErrorOr<Success>>;
    public class EditProductSellDiscountCommandHandler : IRequestHandler<EditProductSellDiscountCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(EditProductSellDiscountCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
