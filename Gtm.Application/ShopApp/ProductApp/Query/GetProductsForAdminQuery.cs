using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Application.StoresServiceApp.StroreApp;
using Gtm.Application.StoresServiceApp.StroreApp.Query;
using Gtm.Contract.ProductContract.Command;
using Gtm.Contract.ProductContract.Query;
using Gtm.Contract.StoresContract.StoreContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetProductsForAdminQuery(int pageId, int take, int categoryId, string filter, ProductAdminOrderBy orderBy) : IRequest<ErrorOr<ProductAdminPaging>>;
    public class GetProductsForAdminQueryHandler : IRequestHandler<GetProductsForAdminQuery, ErrorOr<ProductAdminPaging>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _categoryRepository;
        public GetProductsForAdminQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ErrorOr<ProductAdminPaging>> Handle(GetProductsForAdminQuery request, CancellationToken cancellationToken)
        {

            string pageTitle = "لیست محصولات";

            // دریافت کوئری پایه
            var res =   _productRepository.GetProductWithRelations();
            res = res.Include(p => p.ProductCategoryRelations);

            // فیلتر بر اساس دسته‌بندی
            if (request.categoryId > 0)
            {
                var category = await _categoryRepository.GetByIdAsync(request.categoryId);
                if (category != null)
                {
                    res = res.Where(r => r.ProductCategoryRelations.Any(o => o.ProductCategoryId == request.categoryId));
                    pageTitle += " برای دسته بندی " + category.Title;
                }
            }

            // فیلتر بر اساس متن جستجو
            if (!string.IsNullOrEmpty(request.filter))
            {
                var lowerFilter = request.filter.ToLower();
                res = res.Where(r => r.Title.ToLower().Contains(lowerFilter) ||
                                    r.Description.ToLower().Contains(lowerFilter));
                pageTitle += " شامل عبارت: " + request.filter;
            }

            // مرتب‌سازی
            switch (request.orderBy)
            {
                case ProductAdminOrderBy.تاریخ_ثبت_از_اول:
                    res = res.OrderBy(p => p.Id);
                    break;
                case ProductAdminOrderBy.تاریخ_ثبت_از_آخر:
                    res = res.OrderByDescending(p => p.Id);
                    break;
            }

            // ایجاد مدل صفحه‌بندی
            var model = new ProductAdminPaging();
            model.GetData(res, request.pageId, request.take, 2);
            model.Filter = request.filter;
            model.CategoryId = request.categoryId;
            model.PageTitle = pageTitle;
            model.OrderBy = request.orderBy;

            // دریافت محصولات برای صفحه جاری
            if (await _productRepository.ExistsAsync(p => true)) // یا هر شرط دیگری
            {
                model.Products = await res.Skip(model.Skip).Take(model.Take)
                    .Select(p => new ProductQueryAdminModel
                    {
                        Active = p.IsActive,
                        ImageAlt = p.ImageAlt,
                        CreationDate = p.CreateDate.ToPersainDate(),
                        UpdateDate = p.UpdateDate.ToPersainDate(),
                        Id = p.Id,
                        ImageName = p.ImageName,
                        Slug = p.Slug,
                        Title = p.Title,
                        Weight = p.Weight,
                    }).ToListAsync();
            }
            else
            {
                model.Products = new List<ProductQueryAdminModel>();
            }

            return model;
        }
    }

}
