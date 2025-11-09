using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.SeoApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.ProductContract.Query;
using Gtm.Contract.ProductFeautreContract.Query;
using Gtm.Contract.ProductGalleryContract.Query;
using Gtm.Contract.SeoContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetSingleProductForUiQuery(int id) : IRequest<ErrorOr<SingleProductUIQueryModel>>;

    public class GetSingleProductForUiQueryHandler : IRequestHandler<GetSingleProductForUiQuery, ErrorOr<SingleProductUIQueryModel>>
    {
        private readonly ISeoRepository _seoRepository;
        private readonly ICityRepo _cityRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMediator _mediator;

        public GetSingleProductForUiQueryHandler(
            ISeoRepository seoRepository,
            ICityRepo cityRepository,
            ISellerRepository sellerRepository,
            IProductRepository productRepository,
            IMediator mediator)
        {
            _seoRepository = seoRepository;
            _cityRepository = cityRepository;
            _sellerRepository = sellerRepository;
            _productRepository = productRepository;
            _mediator = mediator;
        }

        public async Task<ErrorOr<SingleProductUIQueryModel>> Handle(GetSingleProductForUiQuery request, CancellationToken cancellationToken)
        {
            // دریافت محصول با روابطش
            var product = await _productRepository.GetProductWithCategoryFeaturesGalleryAndActiveSellsAsync(request.id);
            if (product == null)
                return Error.NotFound(description: "محصول مورد نظر یافت نشد");

            // ایجاد مدل اولیه
            var model = new SingleProductUIQueryModel
            {
                Id = product.Id,
                Title = product.Title,
                Slug = product.Slug,
                Description = product.Description,
                ImageName = product.ImageName,
                ImageAlt = product.ImageAlt,
                Weight = product.Weight,
                BreadCrumb = new(),
                Categories = product.ProductCategoryRelations?
                    .Select(r => new CategoryForProductSingleQueryModel
                    {
                        Title = r.ProductCategory?.Title,
                        Slug = r.ProductCategory?.Slug
                    }).ToList() ?? new(),
                Features = product.ProductFeatures?
                    .Select(f => new FeatureForProductSingleQueryModel
                    {
                        Title = f.Title,
                        Value = f.Value
                    }).ToList() ?? new(),
                Galleries = product.ProductGalleries?
                    .Select(g => new GalleryForProductSingleQueryModel
                    {
                        ImageName = g.ImageName,
                        ImageAlt = g.ImageAlt
                    }).ToList() ?? new(),
                ProductSells = new(),
                // Seo = null // بعداً اضافه می‌کنی
            };

            // دریافت breadcrumb
            var breadCrumbResult = await _mediator.Send(new GetProductBreadCrumbQuery(null, product.Slug));
            model.BreadCrumb = breadCrumbResult.Value;

            // دریافت اطلاعات SEO
            var seo = await _seoRepository.GetSeoForUiAsync(product.Id, WhereSeo.Product, product.Title);
            // اگر لازم داری این قسمت رو فعال کن
           

            // پردازش فروشندگان به صورت سریالی (اجتناب از concurrency error)
            var productSellsList = new List<ProductSellForProductSingleQueryModel>();
            if (product.ProductSells != null)
            {
                foreach (var sell in product.ProductSells.Where(s => s.IsActive))
                {
                    // ✅ اگر Seller از قبل Include شده باشد، مستقیم از آن استفاده می‌شود
                    var seller = sell.Seller ?? await _sellerRepository.GetByIdAsync(sell.SellerId);
                    if (seller == null) continue;

                    var city = await _cityRepository.GetCityWithStateAsync(c =>
                        c.Id == seller.CityId && c.StateId == seller.StateId);

                    productSellsList.Add(new ProductSellForProductSingleQueryModel
                    {
                        Id = sell.Id,
                        ProductId = product.Id,
                        SellerId = sell.SellerId, // ✅ حالا مقدار درست خواهد بود
                        SellerName = seller.Title,
                        SellerAddress = city != null
                            ? $"{city.State?.Title} {city.Title} {seller.Address}"
                            : seller.Address,
                        Price = sell.Price,
                        PriceAfterOff = sell.Price, // اگر خواستی تخفیف رو اضافه کن
                        Amount = sell.Amount,
                        Unit = sell.Unit,
                        Weight = sell.Weight
                    });
                }
            }

            model.ProductSells = productSellsList;

            return model;
        }
    }


}