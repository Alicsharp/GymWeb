using ErrorOr;
using Gtm.Contract.ProductCategoryContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Appliation.Slug;

namespace Gtm.Application.ShopApp.ProductCategoryApp.Command
{
    public record EditProductCategoryCommand(EditProductCategory Command) : IRequest<ErrorOr<Success>>;

    public class EditProductCategoryCommandHandler : IRequestHandler<EditProductCategoryCommand, ErrorOr<Success>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IFileService _fileService;
        private readonly IProductCategoryValidation _validationService;

        public EditProductCategoryCommandHandler(IProductCategoryRepository productCategoryRepository,IFileService fileService,IProductCategoryValidation validationService)
        {
            _productCategoryRepository = productCategoryRepository;
            _fileService = fileService;
            _validationService = validationService;
        }

        public async Task<ErrorOr<Success>> Handle(EditProductCategoryCommand request, CancellationToken cancellationToken)
        {
            var command = request.Command;

            // اعتبارسنجی
            var validationResult = await _validationService.ValidateEditAsync(command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // یافتن دسته‌بندی موجود
            var category = await _productCategoryRepository.GetByIdAsync(command.Id);

            // بررسی تصویر جدید
            string imageName = category.ImageName;
            string oldImageName = category.ImageName;

            if (command.ImageFile != null)
            {
                imageName = await _fileService.UploadImageAsync(command.ImageFile, FileDirectories.ProductCategoryImageFolder);
                if (string.IsNullOrEmpty(imageName))
                {
                    return Error.Failure(nameof(command.ImageFile), "خطا در آپلود تصویر");
                }

                try
                {
                    await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductCategoryImageFolder, 500);
                    await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductCategoryImageFolder, 100);
                }
                catch
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory}{imageName}");
                    return Error.Failure("Image.Resize", "خطا در تغییر سایز تصویر");
                }
            }

            // اعمال تغییرات
            var slug = SlugUtility.GenerateSlug(command.Slug);
            category.Edit(command.Title, slug, imageName, command.ImageAlt);

            // ذخیره تغییرات
            if (await _productCategoryRepository.SaveChangesAsync())
            {
                // حذف تصاویر قدیمی اگر تصویر جدید آپلود شده بود
                if (command.ImageFile != null && !string.IsNullOrEmpty(oldImageName))
                {
                    await DeleteOldImages(oldImageName);
                }
                return Result.Success;
            }
            else
            {
                // حذف تصاویر جدید در صورت خطا
                if (command.ImageFile != null && !string.IsNullOrEmpty(imageName))
                {
                    await DeleteNewImages(imageName);
                }
                return Error.Failure("Category.Update", "خطا در بروزرسانی دسته‌بندی");
            }
        }

        private async Task DeleteOldImages(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory500}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory100}{imageName}");
            }
            catch
            {
                // لاگ خطای حذف تصاویر قدیمی
            }
        }

        private async Task DeleteNewImages(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory500}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory100}{imageName}");
            }
            catch
            {
                // لاگ خطای حذف تصاویر جدید
            }
        }
    }
}
