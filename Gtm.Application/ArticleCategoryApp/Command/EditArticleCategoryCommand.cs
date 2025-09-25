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
            var validationResult = await _articleCategoryValidator.ValidateUpdateAsync(request.CategoryDto);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
            var category = await _articleCategoryRepo.GetByIdAsync(request.CategoryDto.Id);
            if (category is null)
            {
                return Error.NotFound(
                    code: "Category.NotFound",
                    description: "دسته‌بندی مورد نظر یافت نشد");
            }
            string imageName = request.CategoryDto.ImageName;
            string oldImageName = request.CategoryDto.ImageName;
            bool imageChanged = false;
            if (request.CategoryDto.ImageFile !=null)
            {
                
                  imageName = await _fileService.UploadImageAsync(request.CategoryDto.ImageFile, FileDirectories.ArticleCategoryImageFolder); // عتبار سنجی نشده
                if (imageName == null)
                    return Error.Failure("ImapgeUploading", "بارگزاری عکس به شکست خورد");
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleCategoryImageFolder, 400);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleCategoryImageFolder, 100);
                imageChanged = true;

            }
            if(imageChanged) 
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageFolder}{oldImageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory400}{oldImageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory100}{oldImageName}");
            }




            // 4. اعمال تغییرات
            category.Update(
                title: request.CategoryDto.Title,
                imageName:imageName,
                imageAlt: request.CategoryDto.ImageAlt,
               metaDescription:"",
                parentId: request.CategoryDto.Parent);

            // 5. ذخیره تغییرات
            var saveResult = await _articleCategoryRepo.SaveChangesAsync(cancellationToken);
            if (!saveResult)
            {
                return Error.Failure(
                    code: "Database.SaveError",
                    description: "خطا در ذخیره‌سازی تغییرات");
            }
            return Result.Success;
        }
    }
}
