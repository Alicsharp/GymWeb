using ErrorOr;
using Gtm.Contract.ProductCategoryContract.Command;
using Gtm.Domain.ShopDomain.ProductCategoryDomain;
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
    public record ProductCategoryCreateCommand(CreateProductCategory Command) : IRequest<ErrorOr<Success>>;

    public class ProductCategoryCreateCommandHandler : IRequestHandler<ProductCategoryCreateCommand, ErrorOr<Success>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IFileService _fileService;
        private readonly IProductCategoryValidation _validationService;

        public ProductCategoryCreateCommandHandler(IProductCategoryRepository productCategoryRepository, IFileService fileService, IProductCategoryValidation validationService)
        {
            _productCategoryRepository = productCategoryRepository;
            _fileService = fileService;
            _validationService = validationService;
        }

        public async Task<ErrorOr<Success>> Handle(ProductCategoryCreateCommand request, CancellationToken cancellationToken)
        {
            var command = request.Command;

            // اعتبارسنجی
            var validationResult = await _validationService.ValidateCreateAsync(command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // آپلود تصویر
            string imageName = await _fileService.UploadImageAsync(command.ImageFile, FileDirectories.ProductCategoryImageFolder);
            if (string.IsNullOrEmpty(imageName))
            {
                return Error.Failure(nameof(command.ImageFile), "خطا در آپلود تصویر");
            }

            try
            {
                // تغییر سایز تصاویر
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductCategoryImageFolder, 500);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductCategoryImageFolder, 100);

                // ایجاد دسته‌بندی جدید
                var slug = SlugUtility.GenerateSlug(command.Slug);
                var category = new ProductCategory(
                    command.Title.Trim(),
                    slug,
                    imageName,
                    command.ImageAlt,
                    command.Parent);

                // ذخیره دسته‌بندی
                await _productCategoryRepository.AddAsync(category);

                if (await _productCategoryRepository.SaveChangesAsync(cancellationToken))
                {
                    return Result.Success;
                }
                else
                {
                    // حذف تصاویر در صورت خطا
                    await DeleteUploadedImages(imageName);
                    return Error.Failure("Category.Create", "خطا در ایجاد دسته‌بندی");
                }
            }
            catch (Exception ex)
            {
                // حذف تصاویر در صورت خطای غیرمنتظره
                await DeleteUploadedImages(imageName);
                return Error.Failure("Category.Create.Exception", $"خطای سیستمی: {ex.Message}");
            }
        }

        private async Task DeleteUploadedImages(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory500}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductCategoryImageDirectory100}{imageName}");
            }
        }
    }
}