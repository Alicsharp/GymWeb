using ErrorOr;
using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Application.SeoApp;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.ProductContract.Query;
using Gtm.Contract.SeoContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain;
using Gtm.Domain.ShopDomain.ProductDomain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
 

     public record GetProductsForUiQuery(int pageId,string filter,string categorySlug,int Id,ShopOrderBy orderBy) : IRequest<ErrorOr<ShopPaging>>;

    public class GetProductsForUiQueryHandler : IRequestHandler<GetProductsForUiQuery, ErrorOr<ShopPaging>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductCategoryRepository _categoryRepository;
        private readonly ISeoRepository _seoRepository;
        private readonly IMediator _mediator;
        private readonly IProductDiscountRepository _productDiscountRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetProductsForUiQueryHandler(
            IProductRepository productRepository,
            ISellerRepository sellerRepository,
            IProductCategoryRepository categoryRepository,
            ISeoRepository seoRepository,
            IMediator mediator,
            IProductDiscountRepository productDiscountRepository,
            IProductSellRepository productSellRepository)
        {
            _productRepository = productRepository;
            _sellerRepository = sellerRepository;
            _categoryRepository = categoryRepository;
            _seoRepository = seoRepository;
            _mediator = mediator;
            _productDiscountRepository = productDiscountRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<ShopPaging>> Handle(GetProductsForUiQuery request, CancellationToken cancellationToken)
        {
            int ownerSeoId = 0;
            string shopTitle = "محصولات ";
            string seoTitle = "محصولات ";

            var res = _productRepository.GetActiveProducts().OrderBy(p => p.Id);

            // --- فیلتر فروشنده ---
            if (request.Id > 0)
            {
                var seller = await _sellerRepository.GetSellerByIdAsync(request.Id);
                if (seller != null)
                {
                    res = res.Where(r => r.ProductSells.Any(s => s.SellerId == request.Id)).OrderBy(p => p.Id);
                    shopTitle += " فروشگاه " + seller.Title;
                }
            }

            // --- فیلتر دسته‌بندی ---
            if (!string.IsNullOrEmpty(request.categorySlug))
            {
                var category = await _categoryRepository.GetCategoryBySlugAsync(request.categorySlug);
                if (category != null)
                {
                    res = res.Where(r => r.ProductCategoryRelations.Any(s => s.ProductCategoryId == category.Id))
                             .OrderBy(p => p.Id);
                    shopTitle += " دسته بندی " + category.Title;
                    seoTitle = "دسته بندی " + category.Title;
                    ownerSeoId = category.Id;
                }
            }

            // --- فیلتر سرچ ---
            if (!string.IsNullOrEmpty(request.filter))
            {
                var filterLower = request.filter.ToLower();
                res = res.Where(r => r.Title.ToLower().Contains(filterLower) ||
                                     r.Description.ToLower().Contains(filterLower))
                         .OrderBy(p => p.Id);
            }

            // --- مرتب‌سازی ---
            res = request.orderBy switch
            {
                ShopOrderBy.جدید_ترین => res.OrderByDescending(p => p.Id),
                ShopOrderBy.قدیمی_ترین => res.OrderBy(p => p.Id),
                ShopOrderBy.پرفروش_ترین => res.OrderByDescending(p => p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().OrderItems.Count),
                ShopOrderBy.ارزان_ترین => res.OrderBy(p => p.ProductSells.First().Price),
                ShopOrderBy.گران_ترین => res.OrderByDescending(p => p.ProductSells.First().Price),
                _ => res
            };

            // --- ساخت مدل ---
            ShopPaging model = new();
            model.GetData(res, request.pageId, 6, 2);
            model.Filter = request.filter;
            model.ShopId = request.Id;
            model.ShopTitle = shopTitle;
            model.CategorySlug = request.categorySlug;
            model.OrderBy = request.orderBy;
            model.Categories = new();
            model.BreadCrumb = new();
            model.Products = new List<ProductShopUiQueryModel>();

            var productsQueryable = res.Skip(model.Skip).Take(model.Take);

            // materialize کردن محصولات قبل از هر async call روی DbContext
            var productsList = await productsQueryable
                .Select(p => new ProductShopUiQueryModel
                {
                    ImageAlt = p.ImageAlt,
                    Id = p.Id,
                    ImageName = FileDirectories.ProductImageDirectory500 + p.ImageName,
                    Price = p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().Price,
                    PriceAfterOff = p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().Price,
                    Shop = p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().Seller.Title,
                    Slug = p.Slug,
                    Title = p.Title
                })
                .ToListAsync(cancellationToken);

            // اعمال تخفیف‌ها روی محصولات (سریالی برای جلوگیری از concurrency)
            foreach (var product in productsList)
            {
                var discount = await _productDiscountRepository.GetActiveDiscountForProductAsync(product.Id, cancellationToken);
                if (discount != null)
                {
                    if (discount.ProductSellId > 0)
                    {
                        var sellProduct = await _productSellRepository.GetByIdAsync(discount.ProductSellId );
                        var seller = await _sellerRepository.GetSellerByIdAsync(sellProduct.SellerId );
                        product.Shop = seller.Title;
                        product.Price = sellProduct.Price;
                        product.PriceAfterOff = sellProduct.Price - (discount.Percent * sellProduct.Price / 100);
                    }
                    else
                    {
                        product.PriceAfterOff = product.Price - (discount.Percent * product.Price / 100);
                    }
                }
            }

            model.Products = productsList;

            // --- دسته‌بندی‌ها ---
            var categoriesList = await _categoryRepository.GetActiveCategoriesAsync().ToListAsync(cancellationToken);
            model.Categories = categoriesList.Where(c => c.Parent == 0)
                .Select(c => new Gtm.Contract.ProductCategoryContract.Query.ProductCategoryUiQueryModel
                {
                    Title = c.Title,
                    Slug = c.Slug,
                    Childs = categoriesList.Where(d => d.Parent == c.Id)
                                           .Select(d => new Gtm.Contract.ProductCategoryContract.Query.ProductCategoryUiQueryModel
                                           {
                                               Title = d.Title,
                                               Slug = d.Slug,
                                               Childs = null
                                           }).ToList()
                }).ToList();

            // --- بردکرامب ---
            var vBreadCrumb = await _mediator.Send(new GetProductBreadCrumbQuery(model.CategorySlug, ""), cancellationToken);
            model.BreadCrumb = vBreadCrumb.Value;

            // --- سئو ---
            var seo = await _seoRepository.GetSeoForUiAsync(ownerSeoId, WhereSeo.ProductCategory, seoTitle );
            model.Seo = new SeoUiQueryModel(seo.MetaTitle, seo.MetaDescription, seo.MetaKeyWords,
                                            seo.IndexPage, seo.Canonical, seo.Schema);

            return model;
        }
    }

}
