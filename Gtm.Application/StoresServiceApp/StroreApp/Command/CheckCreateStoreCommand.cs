using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.StoresContract.StoreContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.StoresServiceApp.StroreApp.Command
{
    public record CheckCreateStoreCommand(CreateStore model, int userId) : IRequest<ErrorOr<bool>>;

    public class CheckCreateStoreCommandHandler : IRequestHandler<CheckCreateStoreCommand, ErrorOr<bool>>
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly ISellerRepository _sellerRepository;

        public CheckCreateStoreCommandHandler(IProductSellRepository productSellRepository, ISellerRepository sellerRepository)
        {
            _productSellRepository = productSellRepository;
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<bool>> Handle(CheckCreateStoreCommand request, CancellationToken cancellationToken)
        {
            if (request.model.Products.Count < 1)
                return Error.Validation("Store.Products.Empty", "حداقل یک محصول باید انتخاب شود");

            var seller = await _sellerRepository.GetByIdAsync(request.model.SellerId);
            if (seller is null)
                return Error.NotFound("Seller.NotFound", "فروشنده یافت نشد");

            if (seller.UserId != request.userId)
                return Error.Unauthorized("Seller.Ownership.Mismatch", "شما مجوز ایجاد فروشگاه برای این فروشنده را ندارید");

            // دریافت همه محصولات یکجا
            var productIds = request.model.Products.Select(p => p.ProductSellId).ToList();
            var sellProducts = await _productSellRepository.GetByIdsAsync(productIds);

            foreach (var item in request.model.Products)
            {
                var sellProduct = sellProducts.FirstOrDefault(p => p.Id == item.ProductSellId);
                if (sellProduct == null)
                    return Error.NotFound("ProductSell.NotFound", $"محصول با شناسه {item.ProductSellId} یافت نشد");

                if (sellProduct.SellerId != seller.Id)
                    return Error.Conflict("ProductSell.Ownership.Mismatch", $"محصول با شناسه {item.ProductSellId} متعلق به این فروشنده نیست");
            }

            return true;
        }
    }

}
