using ErrorOr;
using Gtm.Contract.ProductCategoryContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.ProductCategoryApp.Query
{
    public record GetProductCategoriesForAdminQuery(int id) : IRequest<ErrorOr<ProductCategoryAdminPageQueryModel>>;
    public class GetProductCategoriesForAdminQueryHandler : IRequestHandler<GetProductCategoriesForAdminQuery, ErrorOr<ProductCategoryAdminPageQueryModel>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductCategoryValidation _productCategoryValidation;

        public GetProductCategoriesForAdminQueryHandler(IProductCategoryRepository productCategoryRepository, IProductCategoryValidation productCategoryValidation)
        {
            _productCategoryRepository = productCategoryRepository;
            _productCategoryValidation = productCategoryValidation;
        }

        public async Task<ErrorOr<ProductCategoryAdminPageQueryModel>> Handle(GetProductCategoriesForAdminQuery request, CancellationToken cancellationToken)
        {
            var validationResults = await _productCategoryValidation.ValidateCategoryExistenceAsync(request.id);
            if (validationResults.IsError)
            {
                return validationResults.Errors;
            }
            List<ProductCategoryAdminQueryModel> productCategories = new List<ProductCategoryAdminQueryModel>();
            string title = "لیست دسته بندی های سر گروه";
            var res = _productCategoryRepository.QueryBy(c => c.Parent == request.id);
            productCategories = res.Select(r => new ProductCategoryAdminQueryModel(r.Id, r.Title, r.ImageName, r.CreateDate.ToPersainDate(), r.UpdateDate.ToPersainDate(), r.IsActive)).ToList();
            if (request.id > 0)
            {
                var category = await _productCategoryRepository.GetByIdAsync(request.id);
                title = $"لیست زیر دسته های {category.Title}";
            }
            ProductCategoryAdminPageQueryModel model = new ProductCategoryAdminPageQueryModel(request.id, title, productCategories);
            return model;
        }
    }
}
