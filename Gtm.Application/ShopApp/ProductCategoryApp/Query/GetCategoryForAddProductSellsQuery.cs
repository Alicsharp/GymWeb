using ErrorOr;
using Gtm.Contract.ProductCategoryContract.Command;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductCategoryApp.Query
{
    public record GetCategoryForAddProductSellsQuery(int id) : IRequest<List<ProductCategoryForAddProductSell>>;
    public class GetCategoryForAddProductSellsQueryHanlder : IRequestHandler<GetCategoryForAddProductSellsQuery, List<ProductCategoryForAddProductSell>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public GetCategoryForAddProductSellsQueryHanlder(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<List<ProductCategoryForAddProductSell>> Handle(GetCategoryForAddProductSellsQuery request, CancellationToken cancellationToken)
        {
            var query = _productCategoryRepository.QueryBy(c => c.Parent == request.id);

            var result = await query.Select(r => new ProductCategoryForAddProductSell
            {
                Id = r.Id,
                Title = r.Title
            }).ToListAsync(cancellationToken);

            return result;


        }
    }
   
}
