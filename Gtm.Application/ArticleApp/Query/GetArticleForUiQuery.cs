using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetArticleForUiQuery(string slug, int pageId, string filter):IRequest<ErrorOr<ArticleUiPaging>>;
    public class GetArticleForUiQueryHandler : IRequestHandler<GetArticleForUiQuery, ErrorOr<ArticleUiPaging>>
    {
        private readonly IArticleRepo  _articleRepo;
        private readonly IArticleCategoryRepo _articleCategoryRepository;

        public GetArticleForUiQueryHandler(IArticleRepo articleRepo, IArticleCategoryRepo articleCategoryRepository)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepository = articleCategoryRepository;
        }

        public async Task<ErrorOr<ArticleUiPaging>> Handle(GetArticleForUiQuery request, CancellationToken cancellationToken)
        {
            string slug = request.slug; // بررسی شود
            string title = "آرشیو مقالات";
            int ownerSeoId = 0;
            string categoryTitle = "آرشیو مقالات";

            // دریافت مقالات فعال با ترتیب نزولی
            var query = _articleRepo.GetAllQueryable()
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.Id);

            // فیلتر بر اساس اسلاگ دسته‌بندی
            if (!string.IsNullOrEmpty(request.slug))
            {
                var category = await _articleCategoryRepository.GetBySlugAsync(request.slug);
                if (category == null)
                {
                    slug = "";
                }
                else
                {
                    query = query.Where(a =>
                        a.CategoryId == category.Id ||
                        a.SubCategoryId == category.Id)
                        .OrderByDescending(a => a.Id);

                    title = $"{title} {category.Title}";
                    ownerSeoId = category.Id;
                    categoryTitle = category.Title;
                }
            }

            // فیلتر بر اساس متن جستجو
            if (!string.IsNullOrEmpty(request.filter))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.filter) ||
                    a.ShortDescription.Contains(request.filter) ||
                    a.Text.Contains(request.filter))
                    .OrderByDescending(a => a.Id);

                title = $"{title} شامل: {request.filter}";
            }

            // ایجاد مدل صفحه‌بندی
            var model = new ArticleUiPaging();
            model.GetData(query, request.pageId, 4, 2);
            model.Filter = request.filter;
            model.Slug = request.slug;
            model.Title = title;
            model.Articles = new List<ArticleCardQueryModel>();

            // اگر مقاله‌ای وجود دارد
            if (await query.AnyAsync(cancellationToken))
            {
                model.Articles = await query
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .Select(b => new ArticleCardQueryModel
                    {
                        ImageAlt = b.ImageAlt,
                        CategoryId = b.SubCategoryId > 0 ? b.SubCategoryId : b.CategoryId,
                        CategoryTitle = "",
                        CategorySlug = "",
                        CreateDate = b.CreateDate.ToPersainDate(),
                        Id = b.Id,
                        ImageName = FileDirectories.ArticleImageDirectory400 + b.ImageName,
                        ShortDescription = b.ShortDescription,
                        Slug = b.Slug,
                        Title = b.Title,
                        Writer = b.Writer
                    })
                    .OrderByDescending(a => a.Id)
                    .ToListAsync(cancellationToken);

                // پر کردن اطلاعات دسته‌بندی‌ها
                foreach (var blog in model.Articles)
                {
                    var category = await _articleCategoryRepository.GetByIdAsync(blog.CategoryId);
                    if (category != null)
                    {
                        blog.CategoryTitle = category.Title;
                        blog.CategorySlug = category.Slug;
                    }
                }
            }

            //// اطلاعات SEO
            //var seo = await _seoRepository.GetSeoForUiAsync(
            //    ownerSeoId,
            //    WhereSeo.ArticleCategory,
            //    categoryTitle);

            //model.Seo = new SeoUiQueryModel(
            //    seo.MetaTitle,
            //    seo.MetaDescription,
            //    seo.MetaKeywords,
            //    seo.IndexPage,
            //    seo.Canonical,
            //    seo.Schema);

            // دریافت دسته‌بندی‌ها
            model.Categories = await GetArticleCategories();

            // دریافت مسیر راهنما (Breadcrumb)
            model.BreadCrumb = await GetArticleBreadCrumb(request.slug);

            return model;
        }

        private async Task<List<BreadCrumbQueryModel>> GetArticleBreadCrumb(string slug)
        {
            var breadcrumbs = new List<BreadCrumbQueryModel>
    {
        new() { Title = "خانه", Url = "/" },
        new() { Title = "مقالات", Url = "/articles" },
        new() { Title = "آرشیو مقالات", Url = "/articles/archive" }
    };

            if (!string.IsNullOrEmpty(slug))
            {
                var category = await _articleCategoryRepository.GetBySlugAsync(slug);
                if (category != null)
                {
                    breadcrumbs.Add(new()
                    {
                        Title = category.Title,
                        Url = $"/articles/category/{slug}"
                    });
                }
            }

            return breadcrumbs;
        }

        private async Task<List<ArticleCategorySearchQueryModel>> GetArticleCategories()
        {
            var parentCategories = await _articleCategoryRepository
                .GetAllByQueryAsync(c => c.IsActive && c.ParentId == null);

            var result = new List<ArticleCategorySearchQueryModel>();

            foreach (var parent in parentCategories)
            {
                // تعداد مقالات در دسته‌بندی والد
                var parentCount = await _articleRepo
                    .CountAsync(a => a.IsActive &&
                        (a.CategoryId == parent.Id || a.SubCategoryId == parent.Id));

                // دریافت زیردسته‌ها
                var childCategories = await _articleCategoryRepository
                    .GetAllByQueryAsync(c => c.IsActive && c.ParentId == parent.Id);

                var childModels = new List<ArticleCategorySearchQueryModel>();

                foreach (var child in childCategories)
                {
                    // تعداد مقالات در هر زیردسته
                    var childCount = await _articleRepo
                        .CountAsync(a => a.IsActive &&
                            (a.CategoryId == child.Id || a.SubCategoryId == child.Id));

                    childModels.Add(new ArticleCategorySearchQueryModel
                    {
                        Id = child.Id,
                        Title = child.Title,
                        Slug = child.Slug,
                        ArticleCount = childCount,
                        Childs = new List<ArticleCategorySearchQueryModel>()
                    });
                }

                result.Add(new ArticleCategorySearchQueryModel
                {
                    Id = parent.Id,
                    Title = parent.Title,
                    Slug = parent.Slug,
                    ArticleCount = parentCount,
                    Childs = childModels
                });
            }

            return result;

        }
    }
}
