using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetProductIdByProductSellIdQuery(int productSellId):IRequest<ErrorOr<int>>;
    public class GetProductIdByProductSellIdQueryHandler : IRequestHandler<GetProductIdByProductSellIdQuery, ErrorOr<int>>
    {
        private readonly IProductSellRepository _productSellRepository;

        public GetProductIdByProductSellIdQueryHandler(IProductSellRepository productSellRepository)
        {
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<int>> Handle(GetProductIdByProductSellIdQuery request, CancellationToken cancellationToken)
        {
            return await _productSellRepository.GetProductIdByProductSellIdAsync(request.productSellId);
        }
    }
}
