using ErrorOr;
using Gtm.Contract.DiscountsContract.ProductDiscountContract.Command;
using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
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
    public record CreateProductDiscountCommand(CreateProductDiscount command): IRequest<ErrorOr<Success>>;

    public class CreateProductDiscountCommandHandler: IRequestHandler<CreateProductDiscountCommand, ErrorOr<Success>>
    {
        private readonly IProductDiscountRepository _productDiscountRepository;

        public CreateProductDiscountCommandHandler(IProductDiscountRepository productDiscountRepository)
        {
            _productDiscountRepository = productDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CreateProductDiscountCommand request, CancellationToken cancellationToken)
        {
            DateTime startDate = request.command.StartDate.ToEnglishDateTime();
            DateTime endDate = request.command.EndDate.ToEnglishDateTime(); // اصلاح شد

            ProductDiscount productDiscount = new ProductDiscount(
                request.command.ProductId,
                0,
                startDate,
                endDate,
                request.command.Percent);

            await _productDiscountRepository.AddAsync(productDiscount);

            var saveResult = await _productDiscountRepository.SaveChangesAsync(cancellationToken);

            if (saveResult)
                return Result.Success;

            return Error.Failure(
                code: nameof(CreateProductDiscountCommand),
                description: ValidationMessages.SystemErrorMessage
            );
        }
    }

}
