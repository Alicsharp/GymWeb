using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryRelationApp;
using Gtm.Contract.ProductContract.Command;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetProductForEditQuery(int id) : IRequest<ErrorOr<EditProduct>>;
    public class GetProductForEditQueryHandler : IRequestHandler<GetProductForEditQuery, ErrorOr<EditProduct>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRelationRepository _productCategoryRelationRepository;
        private readonly IProductValidation _productValidation;

        public GetProductForEditQueryHandler(IProductRepository productRepository,IProductCategoryRelationRepository productCategoryRelationRepository,IProductValidation productValidation)
        {
            _productRepository = productRepository;
            _productCategoryRelationRepository = productCategoryRelationRepository;
            _productValidation = productValidation;
        }

        public async Task<ErrorOr<EditProduct>> Handle(GetProductForEditQuery request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _productValidation.ValidateGetForEditAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var product = await _productRepository.GetByIdAsync(request.id);
            var categories = _productCategoryRelationRepository.QueryBy(c => c.ProductId == request.id);

            return new EditProduct()
            {
                ImageAlt = product.ImageAlt,
                Categoryids = await categories.Select(c => c.ProductCategoryId).ToListAsync(),
                Id = request.id,
                ImageFile = null,
                ImageName = product.ImageName,
                ShortDescription = product.ShortDescription,
                Slug = product.Slug,
                Text = product.Description,
                Title = product.Title,
                Weight = product.Weight
            };
        }
    }
}
