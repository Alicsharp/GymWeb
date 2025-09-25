using ErrorOr;
using Gtm.Contract.DiscountsContract.ProductDiscountContract.Command;
using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using Gtm.Domain.ShopDomain.ProductDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp.Command
{
    public record CreateProductSellDiscountCommand(CreateProductSellDiscount command, int productId): IRequest<ErrorOr<Success>>;

    public class CreateProductSellDiscountCommandHandler: IRequestHandler<CreateProductSellDiscountCommand, ErrorOr<Success>>
    {
        private readonly IProductDiscountRepository _productDiscountRepository;

        public CreateProductSellDiscountCommandHandler(IProductDiscountRepository productDiscountRepository)
        {
            _productDiscountRepository = productDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CreateProductSellDiscountCommand request, CancellationToken cancellationToken)
        {
            DateTime startDate = request.command.StartDate.ToEnglishDateTime();
            DateTime endDate = request.command.EndDate.ToEnglishDateTime(); 

            ProductDiscount productDiscount = new ProductDiscount(
                request.productId,
                request.command.ProductSellId,
                startDate,
                endDate,
                request.command.Percent);

            await _productDiscountRepository.AddAsync(productDiscount);

            var saveResult = await _productDiscountRepository.SaveChangesAsync(cancellationToken);

            if (saveResult)
                return Result.Success;  

            return Error.Failure(       
                code: nameof(CreateProductSellDiscountCommand),
                description: ValidationMessages.SystemErrorMessage
            );
        }
    }

}
