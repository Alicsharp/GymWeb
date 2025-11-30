using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Gtm.Domain.BlogDomain.BlogDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetSingleArticleForUiQuery(string slug) : IRequest<ErrorOr<SingleArticleQueryModel>>;
    public class GetSingleArticleForUiQueryHandler : IRequestHandler<GetSingleArticleForUiQuery, ErrorOr<SingleArticleQueryModel>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleCategoryRepo _articleCategoryRepo;

        public GetSingleArticleForUiQueryHandler(IArticleRepo articleRepo,IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepo = articleCategoryRepo;
        }

        public async Task<ErrorOr<SingleArticleQueryModel>> Handle(GetSingleArticleForUiQuery request, CancellationToken cancellationToken)
        {
            // 1. دریافت مقاله
            var article = await _articleRepo.GetBySlugAsync(request.slug, cancellationToken);
            if (article == null)
                return Error.NotFound(description: "مقاله مورد نظر یافت نشد");

            // 2. دریافت دسته‌بندی‌ها (پدر و فرزند)
            ArticleCategory? parentCategory = null;
            ArticleCategory? subCategory = null;

            // دریافت دسته پدر (اگر دارد)
            if (article.CategoryId > 0)
            {
                parentCategory = await _articleCategoryRepo.GetByIdAsync(article.CategoryId, cancellationToken);
            }

            // دریافت دسته فرزند (اگر دارد و متفاوت از پدر است)
            if (article.SubCategoryId > 0 && article.SubCategoryId != article.CategoryId)
            {
                subCategory = await _articleCategoryRepo.GetByIdAsync(article.SubCategoryId, cancellationToken);
            }

            // 3. انتخاب دسته‌بندی اصلی برای نمایش در ویو (اولویت با زیردسته است)
            var displayCategory = subCategory ?? parentCategory;

            var model = new SingleArticleQueryModel
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Description = article.Text,
                // استفاده از ثابت فایل‌ها
                ImageName = FileDirectories.ArticleImageDirectory400 + article.ImageName,
                ImageAlt = article.ImageAlt,
                CreationDate = article.CreateDate.ToPersianDate(), // فرض: اکستنشن متد ایمن است
                VisitCount = article.VisitCount,

                CategoryId = displayCategory?.Id ?? 0,
                CategoryTitle = displayCategory?.Title ?? "بدون دسته‌بندی",
                CategorySlug = displayCategory?.Slug ?? "",

                // ساخت Breadcrumb با داده‌های آماده (بدون کوئری اضافه)
                BreadCrumb = GetArticleBreadCrumb(article, parentCategory, subCategory)
            };

            return model;
        }

        // متد Breadcrumb دیگه Async نیست چون دیتابیس صدا نمیزنه (بهینه شد)
        private List<BreadCrumbQueryModel> GetArticleBreadCrumb(Article article, ArticleCategory? parent, ArticleCategory? sub)
        {
            var breadcrumbs = new List<BreadCrumbQueryModel>
    {
        new() { Title = "خانه", Url = "/" },
        new() { Title = "مقالات", Url = "/articles" },
        new() { Title = "آرشیو مقالات", Url = "/articles/archive" }
    };

            // 1. افزودن پدر
            if (parent != null)
            {
                breadcrumbs.Add(new()
                {
                    Title = parent.Title,
                    Url = $"/articles/category/{parent.Slug}"
                });
            }

            // 2. افزودن فرزند (اگر وجود داشت)
            if (sub != null)
            {
                breadcrumbs.Add(new()
                {
                    Title = sub.Title,
                    Url = $"/articles/category/{sub.Slug}"
                });
            }

            // 3. افزودن خود مقاله (غیر فعال - بدون لینک)
            breadcrumbs.Add(new() { Title = article.Title, Url = "" });

            return breadcrumbs;
        }
    }
}
