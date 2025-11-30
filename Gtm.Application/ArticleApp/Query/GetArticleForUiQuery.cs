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
    public record GetArticleForUiQuery(string slug, int pageId, string filter) : IRequest<ErrorOr<ArticleUiPaging>>;

    public class GetArticleForUiQueryHandler : IRequestHandler<GetArticleForUiQuery, ErrorOr<ArticleUiPaging>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleCategoryRepo _articleCategoryRepository;

        public GetArticleForUiQueryHandler(IArticleRepo articleRepo, IArticleCategoryRepo articleCategoryRepository)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepository = articleCategoryRepository;
        }

        public async Task<ErrorOr<ArticleUiPaging>> Handle(GetArticleForUiQuery request, CancellationToken cancellationToken)
        {
            string title = "آرشیو مقالات";

            // 1. ساخت کوئری پایه
            var query = _articleRepo.GetAllQueryable()
                .Where(a => a.IsActive);

            // 2. فیلتر دسته‌بندی
            if (!string.IsNullOrEmpty(request.slug))
            {
                // ✅ اصلاح: پاس دادن توکن
                var category = await _articleCategoryRepository.GetBySlugAsync(request.slug, cancellationToken);
                if (category != null)
                {
                    query = query.Where(a => a.CategoryId == category.Id || a.SubCategoryId == category.Id);
                    title = $"{title} {category.Title}";
                }
            }

            // 3. فیلتر متن جستجو
            if (!string.IsNullOrEmpty(request.filter))
            {
                query = query.Where(a =>
                    a.Title.Contains(request.filter) ||
                    a.ShortDescription.Contains(request.filter) ||
                    a.Text.Contains(request.filter));

                title = $"{title} شامل: {request.filter}";
            }

            query = query.OrderByDescending(a => a.Id);

            // 4. تنظیم مدل خروجی
            var model = new ArticleUiPaging
            {
                Filter = request.filter,
                Slug = request.slug,
                Title = title,
                Articles = new List<ArticleCardQueryModel>()
            };

            model.GetData(query.Cast<object>(), request.pageId, 4, 2);

            // 5. واکشی اطلاعات
            if (model.DataCount > 0)
            {
                var rawArticles = await query
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Slug,
                        b.ShortDescription,
                        b.ImageName,
                        b.ImageAlt,
                        b.VisitCount,
                        b.CreateDate,
                        b.Writer,
                        DisplayCategoryId = b.SubCategoryId > 0 ? b.SubCategoryId : b.CategoryId
                    })
                    .ToListAsync(cancellationToken);

                var categoryIds = rawArticles.Select(r => r.DisplayCategoryId).Distinct().ToList();

                // ✅ اصلاح: پاس دادن توکن
                var categoriesInfo = await _articleCategoryRepository.GetAllByQueryAsync(c => categoryIds.Contains(c.Id), cancellationToken);

                var categoryDict = categoriesInfo.ToDictionary(c => c.Id, c => new { c.Title, c.Slug });

                model.Articles = rawArticles.Select(b => new ArticleCardQueryModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    ShortDescription = b.ShortDescription,
                    ImageName = FileDirectories.ArticleImageDirectory400 + b.ImageName,
                    ImageAlt = b.ImageAlt,
                    VisitCount = b.VisitCount,
                    CreateDate = b.CreateDate.ToPersianDate(),
                    Writer = b.Writer,
                    CategoryId = b.DisplayCategoryId,
                    CategoryTitle = categoryDict.ContainsKey(b.DisplayCategoryId) ? categoryDict[b.DisplayCategoryId].Title : "",
                    CategorySlug = categoryDict.ContainsKey(b.DisplayCategoryId) ? categoryDict[b.DisplayCategoryId].Slug : "",
                }).ToList();
            }

            // 6. دریافت سایدبار
            model.Categories = await GetArticleCategoriesOptimized(cancellationToken);

            // 7. بردکرامب
            model.BreadCrumb = await GetArticleBreadCrumb(request.slug, cancellationToken); // توکن اضافه شد

            return model;
        }

        private async Task<List<ArticleCategorySearchQueryModel>> GetArticleCategoriesOptimized(CancellationToken cancellationToken)
        {
            // ✅ اصلاح: پاس دادن توکن
            var allCategories = await _articleCategoryRepository.GetAllByQueryAsync(c => c.IsActive, cancellationToken);

            var articleCounts = await _articleRepo.GetAllQueryable()
                .Where(a => a.IsActive)
                .GroupBy(a => a.SubCategoryId > 0 ? a.SubCategoryId : a.CategoryId)
                .Select(g => new { CatId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var countDict = articleCounts.ToDictionary(k => k.CatId, v => v.Count);

            var result = new List<ArticleCategorySearchQueryModel>();
            var parentCategories = allCategories.Where(c => c.ParentId == null).ToList();

            foreach (var parent in parentCategories)
            {
                var childs = allCategories.Where(c => c.ParentId == parent.Id).ToList();
                var childModels = new List<ArticleCategorySearchQueryModel>();

                foreach (var child in childs)
                {
                    int count = countDict.ContainsKey(child.Id) ? countDict[child.Id] : 0;
                    childModels.Add(new ArticleCategorySearchQueryModel
                    {
                        Id = child.Id,
                        Title = child.Title,
                        Slug = child.Slug,
                        ArticleCount = count,
                        Childs = new List<ArticleCategorySearchQueryModel>()
                    });
                }

                int parentCount = countDict.ContainsKey(parent.Id) ? countDict[parent.Id] : 0;
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

        // ✅ اصلاح: دریافت توکن در ورودی متد
        private async Task<List<BreadCrumbQueryModel>> GetArticleBreadCrumb(string slug, CancellationToken cancellationToken)
        {
            var breadcrumbs = new List<BreadCrumbQueryModel>
            {
                new() { Title = "خانه", Url = "/" },
                new() { Title = "مقالات", Url = "/articles" },
                new() { Title = "آرشیو مقالات", Url = "/articles/archive" }
            };

            if (!string.IsNullOrEmpty(slug))
            {
                // ✅ اصلاح: پاس دادن توکن
                var category = await _articleCategoryRepository.GetBySlugAsync(slug, cancellationToken);
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
    }

}
