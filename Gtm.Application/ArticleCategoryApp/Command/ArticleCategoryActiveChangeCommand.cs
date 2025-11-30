using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Command
{
    public record ArticleCategoryActiveChangeCommand(int Id):IRequest<ErrorOr<Success>>;
    public class ArticleCategoryActiveChangeCommandHandler : IRequestHandler<ArticleCategoryActiveChangeCommand, ErrorOr<Success>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public ArticleCategoryActiveChangeCommandHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<Success>> Handle(ArticleCategoryActiveChangeCommand request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی
            var validationResults = await _articleCategoryValidator.ValidateIdAsync(request.Id);
            if (validationResults.IsError)
            {
                return validationResults.Errors;
            }

            // 2. دریافت از دیتابیس (با توکن)
            var entity = await _articleCategoryRepo.GetByIdAsync(request.Id, cancellationToken);

            // 3. اگر پیدا نشد (Guard Clause)
            if (entity is null)
            {
                return Error.NotFound("ArticleCategory.NotFound", "دسته‌بندی مورد نظر یافت نشد.");
            }

            // 4. تغییر وضعیت
            entity.ActivationChange();

            // 5. ذخیره
            var result = await _articleCategoryRepo.SaveChangesAsync(cancellationToken);
            if (result)
            {
                return Result.Success;
            }

            return Error.Failure("ActiveChange", "عملیات فعال یا غیر فعال سازی شکست خورد");
        }
    }
}
