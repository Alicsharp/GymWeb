using ErrorOr;
using Gtm.Contract.ProductSellContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetProductsForAddProductSellsQuery(int id):IRequest<List<ProductForAddProductSellQueryModel>>;
    public class GetProductsForAddProductSellsQueryHandler : IRequestHandler<GetProductsForAddProductSellsQuery, List<ProductForAddProductSellQueryModel>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsForAddProductSellsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductForAddProductSellQueryModel>> Handle(GetProductsForAddProductSellsQuery request, CancellationToken cancellationToken)
        {
            return await _productRepository.GetProductsByCategoryIdAsync(request.id);
            

        }
    }
}
