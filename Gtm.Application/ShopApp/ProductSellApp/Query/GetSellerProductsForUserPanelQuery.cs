using ErrorOr;
using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.ProductSellContract.Query;
using Gtm.Contract.SellerContract.Command;
using Gtm.Contract.SellerContract.Query;
using Gtm.Domain.ShopDomain.SellerDomain;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.ProductSellApp.Query
{
    public record GetSellerProductsForUserPanelQuery(int pageId, string filter, int sellerId, int userId)
        : IRequest<ErrorOr<SellerProductPageUserPanelQueryModel>>;

    public class GetSellerProductsForUserPanelQueryHandler
        : IRequestHandler<GetSellerProductsForUserPanelQuery, ErrorOr<SellerProductPageUserPanelQueryModel>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductDiscountRepository _productDiscountRepository;

        public GetSellerProductsForUserPanelQueryHandler(
            ISellerRepository sellerRepository,
            IProductSellRepository productSellRepository,
            IProductDiscountRepository productDiscountRepository)
        {
            _sellerRepository = sellerRepository;
            _productSellRepository = productSellRepository;
            _productDiscountRepository = productDiscountRepository;
        }

        public async Task<ErrorOr<SellerProductPageUserPanelQueryModel>> Handle(GetSellerProductsForUserPanelQuery request, CancellationToken cancellationToken)
        {
            var seller = await _sellerRepository.GetSellerForUserPanelAsync(request.sellerId, request.userId);
            if (seller == null) return Error.NotFound();

            var res = _productSellRepository.GetProductSellsForSeller(request.sellerId, request.filter);

            SellerProductPageUserPanelQueryModel model = new();
            model.GetData(res, request.pageId, 15, 2);
            model.Filter = request.filter;
            model.SellerId = request.sellerId;
            model.SellerTitle = seller.Title;
            model.Products = new();

            if (res.Any())
            {
                model.Products = await res.Skip(model.Skip).Take(model.Take)
                    .Select(r => new ProductSellUserPanelQueryModel
                    {
                        Active = r.IsActive,
                        Amount = r.Amount,
                        Id = r.Id,
                        Price = r.Price,
                        ProductId = r.ProductId,
                        ProductImageName = r.Product.ImageName,
                        ProductSlug = r.Product.Slug,
                        ProductTitle = r.Product.Title,
                        SellCount = r.OrderItems
                            .Where(i => i.OrderSeller.Status == OrderSellerStatus.پرداخت_شده ||
                                        i.OrderSeller.Status == OrderSellerStatus.در_حال_آماده_سازی ||
                                        i.OrderSeller.Status == OrderSellerStatus.ارسال_شده)
                            .Sum(o => o.Count),
                        SellerId = r.SellerId,
                        Unit = r.Unit,
                        Weight = r.Weight,

                        // پیش‌فرض
                        ProductDiscountPercent = 0,
                        ProductSellDiscountPercent = 0,
                        PriceAfterDiscount = r.Price
                    }).ToListAsync(cancellationToken);

                // اعمال تخفیف‌ها
                foreach (var x in model.Products)
                {
                    var discounts = await _productDiscountRepository
                        .GetActiveDiscountsForProductAsync(x.ProductId, x.Id, cancellationToken);

                    // تخفیف محصول
                    var productDiscount = discounts
                        .Where(p => p.ProductId == x.ProductId && p.ProductSellId == 0)
                        .OrderBy(p => p.Id)
                        .LastOrDefault();

                    if (productDiscount != null)
                    {
                        x.ProductDiscountPercent = productDiscount.Percent;
                        x.PriceAfterDiscount -= (productDiscount.Percent * x.Price / 100);
                    }

                    // تخفیف فروش محصول
                    var productSellDiscount = discounts
                        .Where(p => p.ProductId == x.ProductId && p.ProductSellId == x.Id)
                        .OrderBy(p => p.Id)
                        .LastOrDefault();

                    if (productSellDiscount != null)
                    {
                        x.ProductSellDiscountPercent = productSellDiscount.Percent;
                        x.PriceAfterDiscount -= (productSellDiscount.Percent * x.Price / 100);
                    }
                }
            }

            return model;
        }
    }

}
