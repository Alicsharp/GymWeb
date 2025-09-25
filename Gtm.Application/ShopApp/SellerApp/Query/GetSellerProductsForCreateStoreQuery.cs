using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.ProductContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellerProductsForCreateStoreQuery(int id, int userId) : IRequest<ErrorOr<List<ProductForAddStoreQueryModel>>>;
    public class GetSellerProductsForCreateStoreQueryHandler : IRequestHandler<GetSellerProductsForCreateStoreQuery, ErrorOr<List<ProductForAddStoreQueryModel>>>
    {
        private readonly ISellerRepository _sellerRepository;

        private readonly IProductSellRepository _productSellRepository;

        public GetSellerProductsForCreateStoreQueryHandler(ISellerRepository sellerRepository, IProductSellRepository productSellRepository)
        {
            _sellerRepository = sellerRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<List<ProductForAddStoreQueryModel>>> Handle(GetSellerProductsForCreateStoreQuery request, CancellationToken cancellationToken)
        {
            var seller = await _sellerRepository.GetByIdAsync(request.id);
            if (seller == null || seller.UserId != request.userId) return Error.Failure("شکست");

            var products = await _productSellRepository.GetProductsForStoreCreation(request.id);

            return products;
        }
    }
}
