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
            var validationResult = await _articleCategoryValidator.ValidateIdAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var entity = await _articleCategoryRepo.GetByIdAsync(request.Id);
            if (entity == null)
            {
                return Error.NotFound("Category.NotFound", "دسته‌بندی با این شناسه پیدا نشد.");
            }

            // اگر دسته‌بندی خودش فرزند است، خطا بده
            if (entity.ParentId != null)
            {
                return Error.Failure("HaveParent", "شناسه داده‌شده خودش فرزند است و والد ندارد.");
            }

            return Result.Success;
        }

    }
}
