using ErrorOr;
using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.ProductContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.ProductVisitApp.Query
{
    /// <summary>
    /// کوئری برای دریافت لیست پربازدیدترین محصولات برای صفحه اصلی
    /// </summary>
    public record GetMostVisitedProductsForIndexQuery()
        : IRequest<ErrorOr<List<ProductCartForIndexQueryModel>>>;
    public class GetMostVisitedProductsForIndexQueryHandler
    : IRequestHandler<GetMostVisitedProductsForIndexQuery, ErrorOr<List<ProductCartForIndexQueryModel>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductDiscountRepository _productDiscountdiscountRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetMostVisitedProductsForIndexQueryHandler(
            IProductRepository productRepository,
            IProductDiscountRepository productdiscountRepository,
            IProductSellRepository productSellRepository)
        {
            _productRepository = productRepository;
            _productDiscountdiscountRepository = productdiscountRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<List<ProductCartForIndexQueryModel>>> Handle(
            GetMostVisitedProductsForIndexQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی بهینه 10 محصول پربازدید
            // (تغییر اصلی اینجاست)
            var products = await _productRepository.GetTop10MostVisitedAsync();

            if (products == null || !products.Any())
            {
                return Error.NotFound(description: "محصول پربازدیدی یافت نشد.");
            }

            // 2. مپ کردن اولیه (دقیقاً طبق منطق کد اصلی شما)
            var model = products.Select(s => new ProductCartForIndexQueryModel
            {
                Id = s.Id,
                Title = s.Title,
                Amount = s.ProductSells.Sum(s => s.Amount),
                ImageAlt = s.ImageAlt,
                PriceAfterOff = s.ProductSells.First().Price,
                ImageName = FileDirectories.ProductImageDirectory500 + s.ImageName,
                Price = s.ProductSells.First().Price,
                Shop = s.ProductSells.First().Seller.Title,
                Slug = s.Slug,
                isWishList = false
            }).ToList();

            // --- حل مشکل N+1 تخفیف (دقیقاً مانند کد قبلی) ---

            // 3. گرفتن تمام تخفیف‌های مرتبط (فقط 1 کوئری به _discountContext)
            var productIds = model.Select(m => m.Id).ToList();
            var allActiveDiscounts = await _productDiscountdiscountRepository.GetActiveDiscountsForProductsAsync(productIds);

            if (!allActiveDiscounts.Any())
            {
                return model; // اگر هیچ تخفیفی نبود، لیست را برگردان
            }

            // 4. گرفتن بهترین تخفیف برای هر محصول در حافظه
            var bestDiscounts = allActiveDiscounts
                .GroupBy(d => d.ProductId)
                .Select(g => g.First())
                .ToDictionary(d => d.ProductId);

            // 5. گرفتن تمام ProductSells خاص (فقط 1 کوئری به _shopContext)
            var productSellIdsToFetch = bestDiscounts.Values
                .Where(d => d.ProductSellId > 0)
                .Select(d => d.ProductSellId)
                .ToList();

            var discountedSells = (await _productSellRepository.GetSellsWithSellerAsync(productSellIdsToFetch))
                .ToDictionary(ps => ps.Id);

            // 6. اعمال تخفیف‌ها در حافظه (جایگزین حلقه N+1)
            foreach (var item in model)
            {
                if (bestDiscounts.TryGetValue(item.Id, out var discount))
                {
                    if (discount.ProductSellId > 0)
                    {
                        if (discountedSells.TryGetValue(discount.ProductSellId, out var sellProduct))
                        {
                            item.Shop = sellProduct.Seller.Title;
                            item.Price = sellProduct.Price;
                            item.PriceAfterOff = sellProduct.Price - discount.Percent * sellProduct.Price / 100;
                        }
                    }
                    else
                    {
                        item.PriceAfterOff = item.Price - discount.Percent * item.Price / 100;
                    }
                }
            }

            return model;
        }
    }
}
