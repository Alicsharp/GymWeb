using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ProductContract.Command;
using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using Gtm.Domain.ShopDomain.ProductDomain;
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
    public record CreateProductCommand(CreateProduct Command) : IRequest<ErrorOr<bool>>;

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ErrorOr<bool>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IFileService _fileService;
        private readonly IProductValidation _productValidation;

        public CreateProductCommandHandler(IProductRepository productRepository,IProductCategoryRepository productCategoryRepository,IFileService fileService,IProductValidation productValidation)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _fileService = fileService;
            _productValidation = productValidation;
        }

        public async Task<ErrorOr<bool>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var command = request.Command;

            // اعتبارسنجی
            var validationResult = await _productValidation.ValidateCreateAsync(command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var slug = SlugUtility.GenerateSlug(command.Slug);

            // آپلود تصویر
            string imageName = await _fileService.UploadImageAsync(command.ImageFile, FileDirectories.ProductImageFolder);
            if (string.IsNullOrEmpty(imageName))
            {
                return Error.Failure(
                    code: nameof(command.ImageFile),
                    description: "خطا در آپلود تصویر");
            }

            try
            {
                Product product = new(
                    command.Title.Trim(),
                    slug,
                    command.ShortDescription,
                    command.Text,
                    imageName,
                    command.ImageAlt,
                    command.Weight);

                // تغییر سایز تصاویر
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductImageFolder, 500);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductImageFolder, 100);

                List<ProductCategoryRelation> rels = new();
                foreach (var item in command.Categoryids)
                {
                    ProductCategoryRelation relation = new ProductCategoryRelation(item);
                    rels.Add(relation);
                }
                product.EditCategoryRelations(rels);

                await _productRepository.AddAsync(product);

                // ذخیره محصول
                if (!await _productRepository.SaveChangesAsync(cancellationToken))
                {
                    // حذف تصاویر در صورت خطا
                    await DeleteUploadedImages(imageName);
                    return Error.Failure(
                        code: "Product.Create",
                        description: "خطا در ایجاد محصول");
                }

                return true;
            }
            catch (Exception ex)
            {
                // حذف تصاویر در صورت خطا
                await DeleteUploadedImages(imageName);
                return Error.Failure(
                    code: "Product.Create.Exception",
                    description: $"خطای سیستمی: {ex.Message}");
            }
        }

        private async Task DeleteUploadedImages(string imageName)
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
