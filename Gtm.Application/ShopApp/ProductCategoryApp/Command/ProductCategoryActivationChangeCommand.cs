using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductCategoryApp.Command
{
    public record ProductCategoryActivationChangeCommand(int id) : IRequest<ErrorOr<Success>>;
    public class ProductCategoryActivationChangeCommandHandler : IRequestHandler<ProductCategoryActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductCategoryValidation _validationService;

        public ProductCategoryActivationChangeCommandHandler(IProductCategoryRepository productCategoryRepository,IProductCategoryValidation validationService)
        {
            _productCategoryRepository = productCategoryRepository;
            _validationService = validationService;
        }

        public async Task<ErrorOr<Success>> Handle(ProductCategoryActivationChangeCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validationService.ValidateActivationChangeAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // یافتن دسته‌بندی
            var category = await _productCategoryRepository.GetByIdAsync(request.id);

            // تغییر وضعیت فعال‌سازی
            category.ActivationChange();

            // ذخیره تغییرات
            var saveResult = await _productCategoryRepository.SaveChangesAsync();
            if (saveResult)
            {
                return Result.Success;
            }
            else
            {
                return Error.Failure(
                    "ProductCategory.ActivationChangeFailed",
                    "خطا در تغییر وضعیت فعال‌سازی دسته‌بندی");
            }
        }
    }
}
