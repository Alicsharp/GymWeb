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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    //public record GetProductsForUiQuery(int pageId, string filter, string categorySlug, int Id, ShopOrderBy orderBy) : IRequest<ErrorOr<ShopPaging>>;
    //public class GetProductsForUiQueryHandler : IRequestHandler<GetProductsForUiQuery, ErrorOr<ShopPaging>>
    //{
    //    private readonly IProductRepository _productRepository;
    //    private readonly ISellerRepository _sellerRepository;
    //    private readonly IProductCategoryRepository _categoryRepository;

    //    public GetProductsForUiQueryHandler(IProductRepository productRepository, ISellerRepository sellerRepository, IProductCategoryRepository categoryRepository)
    //    {
    //        _productRepository = productRepository;
    //        _sellerRepository = sellerRepository;
    //        _categoryRepository = categoryRepository;
    //    }

    //    public async Task<ErrorOr<ShopPaging>> Handle(GetProductsForUiQuery request, CancellationToken cancellationToken)
    //    {
    //        string shopTitle = "";
    //        var res = _productRepository.GetActiveProducts().OrderBy(p => p.Id);

    //        if (request.Id > 0)
    //        {
    //            var seller = await _sellerRepository.GetSellerByIdAsync(request.Id);
    //            if (seller != null)
    //            {
    //                res = res.Where(r => r.ProductSells.Any(s => s.SellerId == request.Id)).OrderBy(p => p.Id);
    //                shopTitle = seller.Title;
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(request.categorySlug))
    //        {
    //            var category = await _categoryRepository.GetCategoryBySlugAsync(request.categorySlug);
    //            if (category != null)
    //            {
    //                res = res.Where(r => r.ProductCategoryRelations.Any(s => s.ProductCategoryId == category.Id))
    //                         .OrderBy(p => p.Id);
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(request.filter))
    //        {
    //            res = res.Where(r => r.Title.ToLower().Contains(request.filter.ToLower()) ||
    //                                 r.Description.ToLower().Contains(request.filter.ToLower()))
    //                     .OrderBy(p => p.Id);
    //        }

    //        switch orderBy...
    //        switch (request.orderBy)
    //        {
    //            case ShopOrderBy.جدید_ترین:
    //                res = res.OrderByDescending(p => p.Id);
    //                break;
    //            case ShopOrderBy.قدیمی_ترین:
    //                res = res.OrderBy(p => p.Id);
    //                break;
    //            case ShopOrderBy.پرفروش_ترین:
    //                res = res.OrderByDescending(p => p.ProductSells.Sum(s => s.OrderItems.Count()));
    //                break;
    //            case ShopOrderBy.ارزان_ترین:
    //                res = res.OrderBy(p => p.ProductSells.First().Price);
    //                break;
    //            case ShopOrderBy.گران_ترین:
    //                res = res.OrderByDescending(p => p.ProductSells.First().Price);
    //                break;
    //        }

    //        ShopPaging model = new();
    //        model.GetData(res, request.pageId, 6, 2);
    //        model.Filter = request.filter;
    //        model.ShopId = request.Id;
    //        model.ShopTitle = shopTitle;
    //        model.CategorySlug = request.categorySlug;
    //        model.OrderBy = request.orderBy;
    //        model.Categories = new();
    //        model.BreadCrumb = new();
    //        model.Products = new List<ProductShopUiQueryModel>();

    //        if (res.Any())
    //        {
    //            model.Products = res.Skip(model.Skip).Take(model.Take)
    //                .Select(p => new ProductShopUiQueryModel
    //                {
    //                    ImageAlt = p.ImageAlt,
    //                    PriceAfterOff = p.ProductSells.First().Price,
    //                    Id = p.Id,
    //                    ImageName = p.ImageName,
    //                    Price = p.ProductSells.First().Price,
    //                    Shop = p.ProductSells.First().Seller.Title,
    //                    Slug = p.Slug,
    //                    Title = p.Title
    //                }).ToList();
    //        }

    //        return model;
    //    }
    //}

    public record GetProductsForUiQuery(int pageId,string filter,string categorySlug,int Id,ShopOrderBy orderBy) : IRequest<ErrorOr<ShopPaging>>;

    public class GetProductsForUiQueryHandler: IRequestHandler<GetProductsForUiQuery, ErrorOr<ShopPaging>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductCategoryRepository _categoryRepository;
        private readonly ISeoRepository _seoRepository;
        private readonly IMediator _mediator;
        private readonly IProductDiscountRepository _productDiscountRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetProductsForUiQueryHandler(IProductRepository productRepository, ISellerRepository sellerRepository, IProductCategoryRepository categoryRepository, ISeoRepository seoRepository, IMediator mediator, IProductDiscountRepository productDiscountRepository)
        {
            _productRepository = productRepository;
            _sellerRepository = sellerRepository;
            _categoryRepository = categoryRepository;
            _seoRepository = seoRepository;
            _mediator = mediator;
            _productDiscountRepository = productDiscountRepository;
        }

        public async Task<ErrorOr<ShopPaging>> Handle(GetProductsForUiQuery request, CancellationToken cancellationToken)
        {
            int ownerSeoId = 0;
            string shopTitle = "محصولات ";
            string seoTitle = "محصولات ";

            var res = _productRepository.GetActiveProducts().OrderBy(p => p.Id);

            // فیلتر فروشنده
            if (request.Id > 0)
            {
                var seller = await _sellerRepository.GetSellerByIdAsync(request.Id);
                if (seller != null)
                {
                    res = res.Where(r => r.ProductSells.Any(s => s.SellerId == request.Id))
                             .OrderBy(p => p.Id);
                    shopTitle += " فروشگاه " + seller.Title;
                }
            }

            // فیلتر دسته‌بندی
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

            // فیلتر سرچ
            if (!string.IsNullOrEmpty(request.filter))
            {
                res = res.Where(r => r.Title.ToLower().Contains(request.filter.ToLower()) ||
                                     r.Description.ToLower().Contains(request.filter.ToLower()))
                         .OrderBy(p => p.Id);
            }

            // مرتب‌سازی
            switch (request.orderBy)
            {
                case ShopOrderBy.جدید_ترین:
                    res = res.OrderByDescending(p => p.Id);
                    break;
                case ShopOrderBy.قدیمی_ترین:
                    res = res.OrderBy(p => p.Id);
                    break;
                case ShopOrderBy.پرفروش_ترین:
                    res = res.OrderByDescending(p => p.ProductSells
                                                       .OrderByDescending(b => b.OrderItems.Count)
                                                       .First().OrderItems.Count);
                    break;
                case ShopOrderBy.ارزان_ترین:
                    res = res.OrderBy(p => p.ProductSells.First().Price);
                    break;
                case ShopOrderBy.گران_ترین:
                    res = res.OrderByDescending(p => p.ProductSells.First().Price);
                    break;
            }

            // ساخت مدل
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

            if (res.Any())
            {
                model.Products = res.Skip(model.Skip).Take(model.Take)
                    .Select(p => new ProductShopUiQueryModel
                    {
                        ImageAlt = p.ImageAlt,
                        Id = p.Id,
                        ImageName = FileDirectories.ProductImageDirectory500 + p.ImageName,
                        Price = p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().Price,
                        PriceAfterOff = p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().Price, // تخفیف بعداً اصلاح میشه
                        Shop = p.ProductSells.OrderByDescending(b => b.OrderItems.Count).First().Seller.Title,
                        Slug = p.Slug,
                        Title = p.Title
                    }).ToList();

                // ✅ اضافه کردن تخفیف‌ها
                foreach (var x in model.Products)
                {
                    var discount = await _productDiscountRepository.GetActiveDiscountForProductAsync(x.Id);
                    if (discount != null)
                    {
                        if (discount.ProductSellId > 0)
                        {
                            var sellProduct = await _productSellRepository.GetByIdAsync(discount.ProductSellId);
                            var seller = await _sellerRepository.GetSellerByIdAsync(sellProduct.SellerId);

                            x.Shop = seller.Title;
                            x.Price = sellProduct.Price;
                            x.PriceAfterOff = sellProduct.Price - (discount.Percent * sellProduct.Price / 100);
                        }
                        else
                        {
                            x.PriceAfterOff = x.Price - (discount.Percent * x.Price / 100);
                        }
                    }
                }
            }

            // دسته‌بندی‌ها
            var categories =   _categoryRepository.GetActiveCategoriesAsync();
            model.Categories = categories.Where(c => c.Parent == 0)
                .Select(c => new Gtm.Contract.ProductCategoryContract.Query.ProductCategoryUiQueryModel
                {
                    Title = c.Title,
                    Slug = c.Slug,
                    Childs = categories.Where(d => d.Parent == c.Id)
                                       .Select(d => new Gtm.Contract.ProductCategoryContract.Query.ProductCategoryUiQueryModel
                                       {
                                           Title = d.Title,
                                           Slug = d.Slug,
                                           Childs = null
                                       }).ToList()
                }).ToList();

            // بردکرامب
            var vBreadCrumb = await _mediator.Send(new GetProductBreadCrumbQuery(model.CategorySlug, ""));
            model.BreadCrumb = vBreadCrumb.Value;

            // سئو
            var seo = await _seoRepository.GetSeoForUiAsync(ownerSeoId, WhereSeo.ProductCategory, seoTitle);
            model.Seo = new SeoUiQueryModel(seo.MetaTitle, seo.MetaDescription, seo.MetaKeyWords,
                                            seo.IndexPage, seo.Canonical, seo.Schema);

            return model;
        }

    }

}
