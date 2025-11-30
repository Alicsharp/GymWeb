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
    public record GetArticleForAdminQuery(int id):IRequest<ErrorOr<AdminArticlesPageQueryModel>>;
    public class GetArticleForAdminQueryHandler : IRequestHandler<GetArticleForAdminQuery, ErrorOr<AdminArticlesPageQueryModel>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleValidator _articleValidator;
        private readonly IArticleCategoryRepo _articleCategoryRepo;

        public GetArticleForAdminQueryHandler(IArticleRepo articleRepo, IArticleValidator articleValidator, IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
            _articleCategoryRepo = articleCategoryRepo;
        }

        public async Task<ErrorOr<AdminArticlesPageQueryModel>> Handle(GetArticleForAdminQuery request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی (آیدی نباید منفی باشد. صفر مجاز است چون یعنی "همه")
            if (request.id < 0)
            {
                return Error.Validation("Id.Invalid", "شناسه نامعتبر است");
            }

            // 2. ساخت کوئری اولیه (هنوز به دیتابیس وصل نشده)
            var query = _articleRepo.GetAllQueryable().AsNoTracking();

            // 3. اعمال فیلتر (اگر آیدی خاصی مد نظر بود)
            if (request.id > 0)
            {
                // فقط مقالاتی که در این دسته یا زیردسته هستند
                query = query.Where(b => b.CategoryId == request.id || b.SubCategoryId == request.id);
            }

            // 4. انتخاب فیلدها (Projection) و اجرا
            var articles = await query.Select(b => new ArticleQueryModel
            {
                Active = b.IsActive,
                CategoryId = b.SubCategoryId > 0 ? b.SubCategoryId : b.CategoryId,
                CategoryTitle = "", // اینجا اگر لازم بود باید از Include استفاده کنی
                CreationDate = b.CreateDate.ToPersianDate(),
                Id = b.Id,
                ImageName = $"{FileDirectories.ArticleImageDirectory100}{b.ImageName}",
                Title = b.Title,
                UpdateDate = b.UpdateDate.ToPersianDate(),
                UserId = b.UserId,
                VisitCount = b.VisitCount,
                Writer = b.Writer
            }).ToListAsync(cancellationToken); // توکن را پاس بده

            // 5. تعیین عنوان صفحه
            string pageTitle;
            if (request.id == 0)
            {
                pageTitle = "لیست تمام مقالات";
            }
            else
            {
                var category = await _articleCategoryRepo.GetByIdAsync(request.id);

                // گارد کلاز: اگر دسته‌بندی پیدا نشد
                if (category is null)
                    return Error.NotFound("Category.NotFound", "دسته‌بندی یافت نشد");

                pageTitle = string.IsNullOrEmpty(category.Title)
                    ? "لیست مقالات این دسته‌بندی"
                    : $"لیست زیر دسته‌های {category.Title}";
            }

            return new AdminArticlesPageQueryModel
            {
                CategoryId = request.id,
                PageTitle = pageTitle,
                Article = articles
            };
        }
    }
}
