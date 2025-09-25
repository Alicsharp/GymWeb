using ErrorOr;
using Gtm.Contract.DiscountsContract.ProductDiscountContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp.Query
{
    public record GetForEditProductDiscountCommand(int id):IRequest<ErrorOr<EditProductDiscount>>;
    public class GetForEditProductDiscountCommandHandler : IRequestHandler<GetForEditProductDiscountCommand, ErrorOr<EditProductDiscount>>
    {
        public async Task<ErrorOr<EditProductDiscount>> Handle(GetForEditProductDiscountCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
