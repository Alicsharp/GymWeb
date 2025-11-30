using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Query
{
    public record CheckCategoryHaveParentQuery(int Id):IRequest<ErrorOr<Success>>;
    public class CheckCategoryHaveParentQueryHandler : IRequestHandler<CheckCategoryHaveParentQuery, ErrorOr<Success>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public CheckCategoryHaveParentQueryHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<Success>> Handle(CheckCategoryHaveParentQuery request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی ورودی
            var validationResult = await _articleCategoryValidator.ValidateIdAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // 2. دریافت از دیتابیس (با توکن)
            var entity = await _articleCategoryRepo.GetByIdAsync(request.Id, cancellationToken);

            // 3. بررسی وجود
            if (entity is null)
            {
                return Error.NotFound("Category.NotFound", "دسته‌بندی با این شناسه پیدا نشد.");
            }

            // 4. بررسی قوانین تجاری (بیزینس رول)
            // اگر ParentId نال نباشد، یعنی این خودش یک فرزند است و نمی‌تواند والدِ کس دیگری شود
            if (entity.ParentId != null)
            {
                // پیام خطا را کمی شفاف‌تر کردم
                return Error.Failure("Category.IsChild", "دسته‌بندی انتخاب شده خود یک زیردسته است و نمی‌تواند زیرمجموعه داشته باشد.");
            }

            return Result.Success;
        }

    }
}
