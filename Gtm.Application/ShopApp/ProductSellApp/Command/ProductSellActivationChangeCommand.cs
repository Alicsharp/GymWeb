using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductSellApp.Command
{
    public record ProductSellActivationChangeCommand(int id):IRequest<ErrorOr<Success>>;
    public class ProductSellActivationChangeCommandHandler : IRequestHandler<ProductSellActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductSellValidation _productSellValidation;

        public ProductSellActivationChangeCommandHandler(IProductSellRepository productSellRepository, IProductSellValidation productSellValidation)
        {
            _productSellRepository = productSellRepository;
            _productSellValidation = productSellValidation;
        }

        public async Task<ErrorOr<Success>> Handle(ProductSellActivationChangeCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _productSellValidation.ValidateAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var sell = await _productSellRepository.GetByIdAsync(request.id);
           
            if (sell == null)
            {
                return Error.NotFound(
                    code: "ActivationChange.NotFound",
                    description: "فروش محصول مورد نظر یافت نشد."
                );
            }

            // تغییر وضعیت فعال‌سازی
            sell.ActivationChange();

            // ذخیره تغییرات
            if (await _productSellRepository.SaveChangesAsync(cancellationToken))
            {
                return Result.Success;
            }

            return Error.Failure(
                code: "ActivationChange.SaveFailed",
                description: "متاسفانه در هنگام تغییر وضعیت فعال‌سازی محصول خطایی رخ داد. لطفاً مجدداً تلاش کنید."
            );
        }
    }
}
