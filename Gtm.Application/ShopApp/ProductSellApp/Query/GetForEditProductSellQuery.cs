using ErrorOr;
using Gtm.Contract.ProductSellContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductSellApp.Query
{
    public record GetForEditProductSellQuery(int id):IRequest<ErrorOr<EditProductSell>>;
    public class GetForEditProductSellQueryHandler : IRequestHandler<GetForEditProductSellQuery, ErrorOr<EditProductSell>>
    {
        private readonly IProductSellRepository _productSellRepository;

        public GetForEditProductSellQueryHandler(IProductSellRepository productSellRepository)
        {
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<EditProductSell>> Handle(GetForEditProductSellQuery request, CancellationToken cancellationToken)
        {
            var p = await _productSellRepository.GetByIdAsync(request.id);
            return new EditProductSell()
            {
                Id = p.Id,
                Price = p.Price,
                SellerId = p.SellerId,
                Unit = p.Unit,
                Weight = p.Weight
            };
        }
    }
}
