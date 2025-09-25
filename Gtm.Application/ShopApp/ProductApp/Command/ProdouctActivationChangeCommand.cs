using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductApp.Command
{
    public record ProdouctActivationChangeCommand(int id) : IRequest<ErrorOr<Success>>;
    public class ProdouctActivationChangeCommandHandler : IRequestHandler<ProdouctActivationChangeCommand, ErrorOr<Success>>
    {
     
        private readonly IProductRepository _productRepository;
        private readonly IProductValidation _productValidation;

        public ProdouctActivationChangeCommandHandler(IProductRepository productRepository,IProductValidation productValidation)
        {
            _productRepository = productRepository;
            _productValidation = productValidation;
        }

        public async Task<ErrorOr<Success>> Handle(ProdouctActivationChangeCommand request, CancellationToken cancellationToken)
        {

            // اعتبارسنجی
            var validationResult = await _productValidation.ValidateActivationChangeAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var product = await _productRepository.GetByIdAsync(request.id);
            product.ActivationChange();

            if (await _productRepository.SaveChangesAsync(cancellationToken))
                return Result.Success;

            return Error.Failure(
                code: "Product.ActivationChange",
                description: "خطا در تغییر وضعیت فعالسازی محصول");
        }
    }
}
