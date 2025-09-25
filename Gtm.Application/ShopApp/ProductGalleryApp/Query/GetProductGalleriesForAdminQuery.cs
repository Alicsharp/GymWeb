using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Contract.ProductGalleryContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductGalleryApp.Query
{
    public record GetProductGalleriesForAdminQuery(int productId) : IRequest<ErrorOr<ProductGalleryAdminPageQueryModel>>;
    public class GetProductGalleriesForAdminQueryHandler : IRequestHandler<GetProductGalleriesForAdminQuery, ErrorOr<ProductGalleryAdminPageQueryModel>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductGalleryRepository _productGalleryRepository;
        private readonly IProductGalleryValidation _productGalleryValidation;

        public GetProductGalleriesForAdminQueryHandler(IProductRepository productRepository,IProductGalleryRepository productGalleryRepository,IProductGalleryValidation productGalleryValidation)
        {
            _productRepository = productRepository;
            _productGalleryRepository = productGalleryRepository;
            _productGalleryValidation = productGalleryValidation;
        }

        public async Task<ErrorOr<ProductGalleryAdminPageQueryModel>> Handle(GetProductGalleriesForAdminQuery request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _productGalleryValidation.ValidateGetGalleriesForAdminAsync(request.productId);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var product = await _productRepository.GetByIdAsync(request.productId);
            var galleries = _productGalleryRepository.QueryBy(c => c.ProductId == request.productId);

            return new ProductGalleryAdminPageQueryModel()
            {
                Galleries = galleries.Select(r => new ProductGalleryAdminQueryModel(r.Id, r.ImageName, r.ImageAlt)).ToList(),
                ProductId = request.productId,
                Title = $"گالری تصاویر {product.Title}"
            };
        }
    }
}
