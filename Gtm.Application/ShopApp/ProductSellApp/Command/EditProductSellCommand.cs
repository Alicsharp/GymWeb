using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ProductContract.Command;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Appliation.Slug;
 

namespace Gtm.Application.ShopApp.ProductSellApp.Command
{
    public record EditProductSellCommand(EditProductSell Command) : IRequest<ErrorOr<Success>>;

    public class EditProductSellCommandHandler : IRequestHandler<EditProductSellCommand, ErrorOr<Success>>
    {
       private readonly IProductSellRepository _productSellRepository;

        public EditProductSellCommandHandler(IProductSellRepository productSellRepository)
        {
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<Success>> Handle(EditProductSellCommand request, CancellationToken cancellationToken)
        {
            var command = request.Command;

            // بررسی وجود сущности
            var p = await _productSellRepository.GetByIdAsync(command.Id);
            if (p == null)
                return Error.NotFound(description: "ProductSell not found.");

            // ویرایش مقادیر
            p.Edit(command.Price, command.Unit, command.Weight);

            // ذخیره تغییرات
            if (await _productSellRepository.SaveChangesAsync(cancellationToken))
                return Result.Success;

            // اگر ذخیره موفق نبود، خطای مناسب برگردون
            return Error.Validation(nameof(command.Unit), ValidationMessages.SystemErrorMessage);
        }
    }

}
