using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Query
{
    public record GetArticleCategoryForEditQuery(int Id):IRequest<ErrorOr<UpdateArticleCategoryDto>>;
    public class GetArticleCategoryForEditQueryHandler : IRequestHandler<GetArticleCategoryForEditQuery, ErrorOr<UpdateArticleCategoryDto>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public GetArticleCategoryForEditQueryHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<UpdateArticleCategoryDto>> Handle(GetArticleCategoryForEditQuery request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی
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
                return Error.NotFound("ArticleCategory.NotFound", "دسته بندی مورد نظر یافت نشد");
            }

            // 4. مپ کردن (Mapping)
            return new UpdateArticleCategoryDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Slug = entity.Slug,
                ImageName = entity.ImageName,
                ImageAlt = entity.ImageAlt,
                ImageFile = null, // در حالت ویرایش فایل جدید نال است

                // ✅ اصلاح باگ: جلوگیری از کرش اگر ParentId نال باشد
                Parent = entity.ParentId ?? 0
            };
        }
    }
}
