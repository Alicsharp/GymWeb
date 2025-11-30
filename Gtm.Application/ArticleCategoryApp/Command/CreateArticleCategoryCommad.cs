using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Command;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleCategoryApp.Command
{
    public record CreateArticleCategoryCommand(CreateArticleCategory CategoryDto) : IRequest<ErrorOr<bool>>;

    public class CreateArticleCategoryCommandHandler : IRequestHandler<CreateArticleCategoryCommand, ErrorOr<bool>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _validator;
        private readonly IFileService _fileService;

        public CreateArticleCategoryCommandHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator validator, IFileService fileService)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _validator = validator;
            _fileService = fileService;
        }

        public async Task<ErrorOr<bool>> Handle(CreateArticleCategoryCommand request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی DTO
            var validationResult = await _validator.ValidateCreateAsync(request.CategoryDto);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // 2. اعتبارسنجی والد (Parent)
            // چک می‌کنیم اگر والد دارد، آیا معتبر است و آیا قانون "فقط ۲ سطح" رعایت شده؟
            if (request.CategoryDto.Parent is > 0)
            {
                var parent = await _articleCategoryRepo.GetByIdAsync(request.CategoryDto.Parent.Value, cancellationToken);

                // اگر والد وجود نداشت OR والد خودش فرزند بود (یعنی داریم سطح ۳ می‌سازیم که ممنوع است)
                if (parent == null || parent.ParentId != null)
                {
                    return Error.Validation("Parent.Invalid", "شناسه والد معتبر نیست یا دسته‌بندی انتخاب شده خود یک زیردسته است (حداکثر ۲ سطح مجاز است).");
                }
            }

            // 3. آپلود تصویر
            string imageName = await _fileService.UploadImageAsync(request.CategoryDto.ImageFile, FileDirectories.ArticleCategoryImageFolder);

            if (string.IsNullOrWhiteSpace(imageName))
                return Error.Failure("Image.UploadFailed", "بارگزاری عکس با شکست مواجه شد");

            // ریسایز (بهتر است این متدها هم توکن بگیرند اگر امکانش هست)
            // نکته: اگر اینجا ارور بدهد، عکس اصلی آپلود شده باقی می‌ماند. 
            // در پروژه‌های حساس اینجا Try-Catch می‌گذاریم تا در صورت خطا در ریسایز، عکس اصلی پاک شود.
            await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleCategoryImageFolder, 400);
            await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleCategoryImageFolder, 100);

            // 4. ایجاد موجودیت
            var category = new ArticleCategory(
                title: request.CategoryDto.Title,
                slug: Utility.Appliation.Slug.SlugUtility.GenerateSlug(request.CategoryDto.Title),
                imageName: imageName,
                imageAlt: request.CategoryDto.ImageAlt,
                parentId: request.CategoryDto.Parent);

            // 5. ذخیره در دیتابیس
            await _articleCategoryRepo.AddAsync(category);

            var isSaved = await _articleCategoryRepo.SaveChangesAsync(cancellationToken);

            // 6. مدیریت Rollback (اگر ذخیره نشد، عکس‌ها را پاک کن)
            if (!isSaved)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageFolder}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory400}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory100}{imageName}");

                return Error.Failure("Category.SaveFailed", "ثبت دسته‌بندی در پایگاه داده با خطا مواجه شد.");
            }

            return true;
        }


    }
}
