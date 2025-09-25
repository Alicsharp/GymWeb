using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ProductContract.Command;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetCategoriesForAddProductQuery : IRequest<ErrorOr<List<ProductCategoryForAddProduct>>>;
    public class GetCategoriesForAddProductQueryHadnler : IRequestHandler<GetCategoriesForAddProductQuery, ErrorOr<List<ProductCategoryForAddProduct>>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public GetCategoriesForAddProductQueryHadnler(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<ErrorOr<List<ProductCategoryForAddProduct>>> Handle(GetCategoriesForAddProductQuery request, CancellationToken cancellationToken)
        {
            var res = _productCategoryRepository.GetAllQueryable();
            return await res.Select(r => new ProductCategoryForAddProduct
            {
                Id = r.Id,
                Parent = r.Parent,
                Title = r.Title
            }).ToListAsync();
        }
    }
}
