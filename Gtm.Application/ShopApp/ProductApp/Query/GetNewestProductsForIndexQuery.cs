using ErrorOr;
using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.ProductContract.Query;
using MediatR;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    /// <summary>
    /// کوئری برای دریافت لیست جدیدترین محصولات برای صفحه اصلی
    /// </summary>
    public record GetNewestProductsForIndexQuery()
        : IRequest<ErrorOr<List<ProductCartForIndexQueryModel>>>;
 
public class GetNewestProductsForIndexQueryHandler
    : IRequestHandler<GetNewestProductsForIndexQuery, ErrorOr<List<ProductCartForIndexQueryModel>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductDiscountRepository _productdiscountRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetNewestProductsForIndexQueryHandler(
            IProductRepository productRepository,
            IProductDiscountRepository productddiscountRepository,
            IProductSellRepository productSellRepository)
        {
            _productRepository = productRepository;
            _productdiscountRepository = productddiscountRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<List<ProductCartForIndexQueryModel>>> Handle(
            GetNewestProductsForIndexQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی بهینه 10 محصول جدید
            // (تغییر اصلی اینجاست)
            var products = await _productRepository.GetTop10NewestAsync();

            if (products == null || !products.Any())
            {
                return Error.NotFound(description: "محصول جدیدی یافت نشد.");
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
            var allActiveDiscounts = await _productdiscountRepository.GetActiveDiscountsForProductsAsync(productIds);

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
                            item.PriceAfterOff = sellProduct.Price - (discount.Percent * sellProduct.Price / 100);
                        }
                    }
                    else
                    {
                        item.PriceAfterOff = item.Price - (discount.Percent * item.Price / 100);
                    }
                }
            }

            return model;
        }
    }
}
