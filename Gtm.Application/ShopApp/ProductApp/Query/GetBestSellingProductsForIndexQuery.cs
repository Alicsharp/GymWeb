using ErrorOr;
using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.ProductContract.Query;
using MediatR;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    /// <summary>
    /// کوئری برای دریافت لیست محصولات پرفروش برای صفحه اصلی
    /// </summary>
    public record GetBestSellingProductsForIndexQuery()
        : IRequest<ErrorOr<List<ProductCartForIndexQueryModel>>>;

    public class GetBestSellingProductsForIndexQueryHandler
    : IRequestHandler<GetBestSellingProductsForIndexQuery, ErrorOr<List<ProductCartForIndexQueryModel>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductDiscountRepository _ProductdiscountRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetBestSellingProductsForIndexQueryHandler(
            IProductRepository productRepository,
            IProductDiscountRepository ProductdiscountRepository,
            IProductSellRepository productSellRepository)
        {
            _productRepository = productRepository;
            _ProductdiscountRepository = ProductdiscountRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<List<ProductCartForIndexQueryModel>>> Handle(
            GetBestSellingProductsForIndexQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی بهینه 10 محصول پرفروش
            var products = await _productRepository.GetTop10BestSellingAsync();

            if (products == null || !products.Any())
            {
                return Error.NotFound(description: "محصول پرفروشی یافت نشد.");
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

            // --- حل مشکل N+1 تخفیف ---

            // 3. گرفتن تمام تخفیف‌های مرتبط (فقط 1 کوئری به _discountContext)
            var productIds = model.Select(m => m.Id).ToList();
            var allActiveDiscounts = await _ProductdiscountRepository.GetActiveDiscountsForProductsAsync(productIds);

            if (!allActiveDiscounts.Any())
            {
                return model; // اگر هیچ تخفیفی نبود، لیست را برگردان
            }

            // 4. گرفتن بهترین تخفیف (بالاترین درصد) برای هر محصول در حافظه
            var bestDiscounts = allActiveDiscounts
                .GroupBy(d => d.ProductId)
                .Select(g => g.First()) // (چون از قبل بر اساس درصد مرتب شده‌اند)
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
                        // تخفیف روی یک ProductSell خاص
                        if (discountedSells.TryGetValue(discount.ProductSellId, out var sellProduct))
                        {
                            item.Shop = sellProduct.Seller.Title;
                            item.Price = sellProduct.Price;
                            item.PriceAfterOff = sellProduct.Price - (discount.Percent * sellProduct.Price / 100);
                        }
                    }
                    else
                    {
                        // تخفیف روی کل محصول (اعمال بر همان .First() که در مپ اولیه بود)
                        item.PriceAfterOff = item.Price - (discount.Percent * item.Price / 100);
                    }
                }
            }

            return model;
        }
    }
}
