using ErrorOr;
using Gtm.Contract.ProductCategoryContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductCategoryApp.Query
{
    public record GetProductCategoryForEditQuery(int id) : IRequest<ErrorOr<EditProductCategory>>;
    public class GetProductCategoryForEditQueryHandler : IRequestHandler<GetProductCategoryForEditQuery, ErrorOr<EditProductCategory>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductCategoryValidation _validationService;

        public GetProductCategoryForEditQueryHandler(IProductCategoryRepository productCategoryRepository,IProductCategoryValidation validationService)
        {
            _productCategoryRepository = productCategoryRepository;
            _validationService = validationService;
        }

        public async Task<ErrorOr<EditProductCategory>> Handle(GetProductCategoryForEditQuery request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validationService.ValidateGetForEditAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var category = await _productCategoryRepository.GetByIdAsync(request.id);

            return new EditProductCategory()
            {
                ImageAlt = category.ImageAlt,
                Id = request.id,
                ImageFile = null,
                ImageName = category.ImageName,
                Slug = category.Slug,
                Title = category.Title,
                //Parent = category.Parent // اضافه کردن Parent در صورت نیاز
            };
        }
    }
}
