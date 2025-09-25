using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.SeoApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.ProductContract.Query;
using Gtm.Contract.ProductFeautreContract.Query;
using Gtm.Contract.ProductGalleryContract.Query;
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

        public GetSingleProductForUiQueryHandler(ISeoRepository seoRepository, ICityRepo cityRepository, ISellerRepository sellerRepository, IProductRepository productRepository, IMediator mediator)
        {
            _seoRepository = seoRepository;
            _cityRepository = cityRepository;
            _sellerRepository = sellerRepository;
            _productRepository = productRepository;
            _mediator = mediator;
        }

        public async Task<ErrorOr<SingleProductUIQueryModel>> Handle(GetSingleProductForUiQuery request,CancellationToken cancellationToken)
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
                Seo = null
            };

            // دریافت breadcrumb
            var breadCrumbResult = await _mediator.Send(new GetProductBreadCrumbQuery(null, product.Slug), cancellationToken);
            model.BreadCrumb = breadCrumbResult.Value;

            // دریافت اطلاعات SEO
            var seo = await _seoRepository.GetSeoForUiAsync(product.Id, WhereSeo.Product, product.Title);
            model.Seo = new(
                seo?.MetaTitle,
                seo?.MetaDescription,
                seo?.MetaKeyWords,
                seo?.IndexPage ?? true,
                seo?.Canonical,
                seo?.Schema
            );

            // پردازش فروشندگان به صورت موازی
            var sellerTasks = product.ProductSells?
                .Select(async sell =>
                {
                    var seller = await _sellerRepository.GetByIdAsync(sell.SellerId);
                    if (seller == null) return null;

                    var city = await _cityRepository.GetCityWithStateAsync(c =>
                        c.Id == seller.CityId &&
                        c.StateId == seller.StateId);

                    return new ProductSellForProductSingleQueryModel
                    {
                        SellerId = sell.SellerId,
                        SellerName = seller.Title,
                        ProductId =product.Id, //چیست
                        SellerAddress = city != null ?
                            $"{city.State?.Title} {city.Title} {seller.Address}" :
                            seller.Address,
                        Price = sell.Price,
                        PriceAfterOff = sell.Price - 1,
                        Amount = sell.Amount,
                        Unit = sell.Unit,
                        Weight = sell.Weight
                    };
                }).ToList() ?? new();

            var productSells = await Task.WhenAll(sellerTasks);
            model.ProductSells = productSells.Where(ps => ps != null).ToList();

            return model;
        }

    }
}