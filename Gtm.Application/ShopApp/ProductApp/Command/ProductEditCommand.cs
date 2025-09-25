using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Application.ShopApp.ProductCategoryRelationApp;
using Gtm.Contract.ProductContract.Command;
using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Appliation.Slug;

namespace Gtm.Application.ShopApp.ProductApp.Command
{

    public record ProductEditCommand(EditProduct Command) : IRequest<ErrorOr<Success>>;

    public class ProductEditCommandHandler : IRequestHandler<ProductEditCommand, ErrorOr<Success>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IFileService _fileService;
        private readonly IProductValidation _productValidation;

        public ProductEditCommandHandler(IProductRepository productRepository,IProductCategoryRepository productCategoryRepository,IFileService fileService,IProductValidation productValidation)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _fileService = fileService;
            _productValidation = productValidation;
        }

        public async Task<ErrorOr<Success>> Handle(ProductEditCommand request, CancellationToken cancellationToken)
        {
            var command = request.Command;

            // اعتبارسنجی
            var validationResult = await _productValidation.ValidateEditAsync(command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var product = await _productRepository.GetByIdAsync(command.Id);
            string oldImageName = product.ImageName;
            string imageName = oldImageName;

            try
            {
                // پردازش تصویر جدید اگر وجود دارد
                if (command.ImageFile != null)
                {
                    imageName = await _fileService.UploadImageAsync(command.ImageFile, FileDirectories.ProductImageFolder);
                    if (string.IsNullOrEmpty(imageName))
                    {
                        return Error.Failure(
                            code: nameof(command.ImageFile),
                            description: "خطا در آپلود تصویر");
                    }

                    await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductImageFolder, 500);
                    await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductImageFolder, 100);
                }

                // اعمال تغییرات روی محصول
                var slug = SlugUtility.GenerateSlug(command.Slug);
                product.Edit(
                    command.Title,
                    slug,
                    command.ShortDescription,
                    command.Text,
                    imageName,
                    command.ImageAlt,
                    command.Weight);

                List<ProductCategoryRelation> rels = new();
                foreach (var item in command.Categoryids)
                {
                    ProductCategoryRelation relation = new ProductCategoryRelation(item);
                    rels.Add(relation);
                }
                product.EditCategoryRelations(rels);

                // ذخیره تغییرات
               
                if (await _productRepository.SaveChangesAsync(cancellationToken)) //ایا تنها با سیو چنج تغییرات ذخیره می شوند
                {
                    // حذف تصاویر قدیمی اگر تصویر جدید آپلود شده بود
                    if (command.ImageFile != null)
                    {
                        await DeleteImageAsync(oldImageName);
                    }
                    return Result.Success;
                }
                else
                {
                    // حذف تصاویر جدید اگر ذخیره محصول با خطا مواجه شد
                    if (command.ImageFile != null)
                    {
                        await DeleteImageAsync(imageName);
                    }
                    return Error.Failure(
                        code: "Product.Update",
                        description: "خطا در بروزرسانی محصول");
                }
            }
            catch (Exception ex)
            {
                // حذف تصاویر جدید در صورت بروز خطای غیرمنتظره
                if (command.ImageFile != null && !string.IsNullOrEmpty(imageName) && imageName != oldImageName)
                {
                    await DeleteImageAsync(imageName);
                }
                return Error.Failure(
                    code: "Product.Update.Exception",
                    description: $"خطای سیستمی: {ex.Message}");
            }
        }

        private async Task DeleteImageAsync(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductImageDirectory500}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductImageDirectory100}{imageName}");
            }
        }
    }
}
