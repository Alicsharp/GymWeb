using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Command;
 
using MediatR;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleCategoryApp.Command
{
    public record EditArticleCategoryCommand(UpdateArticleCategoryDto CategoryDto) : IRequest<ErrorOr<Success>>;
    public class EditArticleCategoryCommandHandler : IRequestHandler<EditArticleCategoryCommand, ErrorOr<Success>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _articleCategoryValidator;
        private readonly IFileService _fileService;

        public EditArticleCategoryCommandHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator, IFileService fileService)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
            _fileService = fileService;
        }

        public async Task<ErrorOr<Success>> Handle(EditArticleCategoryCommand request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی
            var validationResult = await _articleCategoryValidator.ValidateUpdateAsync(request.CategoryDto);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // 2. دریافت موجودیت از دیتابیس
            var category = await _articleCategoryRepo.GetByIdAsync(request.CategoryDto.Id, cancellationToken);
            if (category is null)
            {
                return Error.NotFound("Category.NotFound", "دسته‌بندی مورد نظر یافت نشد");
            }

            // نگه داشتن نام عکس فعلی از دیتابیس (نه از DTO)
            string currentImageName = category.ImageName;
            string newImageName = null;
            bool imageChanged = false;

            // 3. مدیریت آپلود عکس جدید
            if (request.CategoryDto.ImageFile != null)
            {
                newImageName = await _fileService.UploadImageAsync(request.CategoryDto.ImageFile, FileDirectories.ArticleCategoryImageFolder);

                if (string.IsNullOrWhiteSpace(newImageName))
                    return Error.Failure("Image.UploadFailed", "بارگزاری عکس با شکست مواجه شد");

                // ریسایز
                await _fileService.ResizeImageAsync(newImageName, FileDirectories.ArticleCategoryImageFolder, 400);
                await _fileService.ResizeImageAsync(newImageName, FileDirectories.ArticleCategoryImageFolder, 100);

                imageChanged = true;
            }

            // 4. اعمال تغییرات روی موجودیت
            category.Update(
                title: request.CategoryDto.Title,
                //// اگر تایتل عوض میشه معمولا اسلاگ هم باید عوض شه
                //slug: Utility.Appliation.Slug.SlugUtility.GenerateSlug(request.CategoryDto.Title),
                imageName: imageChanged ? newImageName : currentImageName, // اگر عکس جدید بود جایگزین کن
                imageAlt: request.CategoryDto.ImageAlt,
                metaDescription: "",
                parentId: request.CategoryDto.Parent);

            // 5. ذخیره در دیتابیس
            var saveResult = await _articleCategoryRepo.SaveChangesAsync(cancellationToken);

            if (saveResult)
            {
                // ✅ موفقیت: حالا ایمن هستیم که عکس قدیمی را پاک کنیم
                if (imageChanged && !string.IsNullOrEmpty(currentImageName))
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageFolder}{currentImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory400}{currentImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory100}{currentImageName}");
                }
                return Result.Success;
            }
            else
            {
                // ❌ شکست: دیتابیس ذخیره نشد، پس عکس جدیدی که آپلود کردیم را پاک می‌کنیم (Rollback)
                if (imageChanged && !string.IsNullOrEmpty(newImageName))
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageFolder}{newImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory400}{newImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory100}{newImageName}");
                }

                return Error.Failure("Database.SaveError", "خطا در ذخیره‌سازی تغییرات");
            }
        }
    }
}
