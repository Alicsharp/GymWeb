using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleCategoryApp.Query
{
    public record GetCategoriesForAdminQuery(int id):IRequest<ErrorOr<ArticleCategoryAdminPageQueryModel>>;
    public class GetCategoriesForAdminQueryHandler : IRequestHandler<GetCategoriesForAdminQuery, ErrorOr<ArticleCategoryAdminPageQueryModel>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo; 
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public GetCategoriesForAdminQueryHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<ArticleCategoryAdminPageQueryModel>> Handle(GetCategoriesForAdminQuery request, CancellationToken cancellationToken)
        {
            string pageTitle = "لیست سر دسته‌های مقاله";

            // 1. تعیین کوئری پایه
            // اگر id > 0 باشد یعنی زیردسته می‌خواهیم، اگر 0 باشد یعنی ریشه
            var query = _articleCategoryRepo.QueryBy(c => true).AsNoTracking();

            if (request.id > 0)
            {
                // اعتبارسنجی فقط برای آیدی‌های مثبت
                var validationResult = await _articleCategoryValidator.ValidateIdAsync(request.id);
                if (validationResult.IsError) return validationResult.Errors;

                // چک کردن وجود والد
                var parentCategory = await _articleCategoryRepo.GetByIdAsync(request.id, cancellationToken);
                if (parentCategory is null)
                {
                    return Error.NotFound("Category.Parent.NotFound", "دسته‌بندی والد یافت نشد.");
                }

                pageTitle = $"لیست زیر دسته‌های {parentCategory.Title}";

                // فیلتر روی والد
                query = query.Where(c => c.ParentId == request.id);
            }
            else
            {
                // فیلتر روی ریشه‌ها
                query = query.Where(c => c.ParentId == null);
            }

            // 2. دریافت داده‌های خام (بدون توابع C# مثل تاریخ شمسی)
            var rawCategories = await query.Select(c => new
            {
                c.Id,
                c.Title,
                c.IsActive,
                c.CreateDate,
                c.UpdateDate,
                c.ImageName
            }).ToListAsync(cancellationToken); // ✅ توکن

            // 3. مپ کردن نهایی در حافظه (In-Memory)
            var viewModels = rawCategories.Select(c => new ArticleCategoryAdminQueryModel
            {
                Id = c.Id,
                Title = c.Title ?? string.Empty,
                Active = c.IsActive,
                ImageName = FileDirectories.ArticleCategoryImageDirectory100 + c.ImageName, // استفاده از ثابت
                CreationDate = c.CreateDate.ToPersianDate(), // ✅ اینجا امن است
                UpdateDate = c.UpdateDate.ToPersianDate()
            }).ToList();

            return new ArticleCategoryAdminPageQueryModel
            {
                Id = request.id,
                PageTitle = pageTitle,
                articleCategories = viewModels
            };
        }
    }
}
