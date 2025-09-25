using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Contract.ProductFeautreContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductFeatureApp.Query
{
    public record GetProductFeaturesForAdminQuery(int productId) : IRequest<ErrorOr<ProductFeaturePageAdminQueryModel>>;
    public class GetProductFeaturesForAdminQueryHandler : IRequestHandler<GetProductFeaturesForAdminQuery, ErrorOr<ProductFeaturePageAdminQueryModel>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductFeatureRepository _productFeatureRepository;
        private readonly IProductFeatureValidation  _productFeatureValidation;

        public GetProductFeaturesForAdminQueryHandler(IProductRepository productRepository,IProductFeatureRepository productFeatureRepository,IProductFeatureValidation productFeatureValidation)
        {
            _productRepository = productRepository;
            _productFeatureRepository = productFeatureRepository;
            _productFeatureValidation = productFeatureValidation;
        }

        public async Task<ErrorOr<ProductFeaturePageAdminQueryModel>> Handle(GetProductFeaturesForAdminQuery request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _productFeatureValidation.ValidateGetFeaturesForAdminAsync(request.productId);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var product = await _productRepository.GetByIdAsync(request.productId);
            var features = _productFeatureRepository.QueryBy(c => c.ProductId == request.productId);

            var featureModels = features.Select(r => new ProductFeatureAdminQueryModel(r.Id, r.Title, r.Value)).ToList();

            return new ProductFeaturePageAdminQueryModel()
            {
                Feautures = featureModels,
                ProductId = request.productId,
                Title = $"لیست ویژگی های {product.Title}"
            };
        }
    }
}
