using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.StoresContract.StoreContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.StoresServiceApp.StroreApp.Query
{
    public record GetStoreDetailForSellerPanelQuery(int userId, int id) : IRequest<ErrorOr<StoreDetailForSellerPanelQueryModel>>;
    public class GetStoreDetailForSellerPanelQueryHadler : IRequestHandler<GetStoreDetailForSellerPanelQuery, ErrorOr<StoreDetailForSellerPanelQueryModel>>
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IStoreRepository _storeRepository;

        public GetStoreDetailForSellerPanelQueryHadler(IProductSellRepository productSellRepository, ISellerRepository sellerRepository, IStoreRepository storeRepository)
        {
            _productSellRepository = productSellRepository;
            _sellerRepository = sellerRepository;
            _storeRepository = storeRepository;
        }

        public async Task<ErrorOr<StoreDetailForSellerPanelQueryModel>> Handle(GetStoreDetailForSellerPanelQuery request, CancellationToken cancellationToken)
        {

            var store = await _storeRepository.GetStoreWithProductsAsync(request.id);
            if (store == null || store.UserId != request.userId) return Error.Failure();
            StoreDetailForSellerPanelQueryModel model = new StoreDetailForSellerPanelQueryModel()
            {
                CreationDate = store.CreateDate.ToPersianDate(),
                Description = store.Description,
                Id = request.id,
                SellerId = store.Id,
                SellerTitle = "",
                StoreProducts = store.StoreProducts.Select(s => new StoreProductDetailForSellerPanelQueryModel
                {
                    Count = s.Count,
                    Id = s.Id,
                    ProductId = 0,
                    ProductSellId = s.ProductSellId,
                    ProductTitle = "",
                    Type = s.Type,
                    Unit = "",
                    ProductImageName = ""
                }).ToList()
            };
            var seller = await _sellerRepository.GetByIdAsync(model.SellerId);
            model.SellerTitle = seller.Title;
            foreach (var x in model.StoreProducts)
            {
                var productsell = await _productSellRepository.GetProductSellWithProductAsync(x.ProductSellId);
                x.ProductId = productsell.ProductId;
                x.ProductTitle = productsell.Product.Title;
                x.Unit = productsell.Unit;
                x.ProductImageName = productsell.Product.ImageName;
            }

            return model;
        }
    }
    
  }
