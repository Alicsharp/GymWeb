using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductFeatureApp.Command
{
    public record DeleteProductFeatureCommand(int id) : IRequest<ErrorOr<Success>>;
    public class DeleteProductFeatureCommandHandler : IRequestHandler<DeleteProductFeatureCommand, ErrorOr<Success>>
    {

        private readonly IProductFeatureRepository _featureRepository;
        private readonly IProductFeatureValidation _featureValidation;

        public DeleteProductFeatureCommandHandler(IProductFeatureRepository featureRepository,IProductFeatureValidation featureValidation)
        {
            _featureRepository = featureRepository;
            _featureValidation = featureValidation;
        }

        public DeleteProductFeatureCommandHandler(IProductFeatureRepository featureRepository)
        {
            _featureRepository = featureRepository;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteProductFeatureCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _featureValidation.ValidateDeleteAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // دریافت ویژگی (با اطمینان از وجود آن)
            var feature = await _featureRepository.GetByIdAsync(request.id);
            _featureRepository.Remove(feature);

            if (await _featureRepository.SaveChangesAsync(cancellationToken))
            {
                return Result.Success;
            }

            return Error.Failure(
                code: "ProductFeature.DeleteFailed",
                description: "خطا در حذف ویژگی محصول");
        }
    }
}
