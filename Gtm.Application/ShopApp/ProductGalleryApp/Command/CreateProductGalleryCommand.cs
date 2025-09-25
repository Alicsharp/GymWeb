using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Contract.ProductGalleryContract.Command;
using Gtm.Contract.ProductGalleryContract.Query;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.ProductGalleryApp.Command
{
    public record CreateProductGalleryCommand(CreateProductGallery Command) : IRequest<ErrorOr<Success>>;
    public class CreateProductGalleryCommandHandler : IRequestHandler<CreateProductGalleryCommand, ErrorOr<Success>>
    {
        private readonly IProductGalleryRepository _productGalleryRepository;
        private readonly IFileService _fileService;
        private readonly IProductGalleryValidation _productGalleryValidation;

        public CreateProductGalleryCommandHandler(IProductGalleryRepository productGalleryRepository,IFileService fileService,IProductGalleryValidation productGalleryValidation)
        {
            _productGalleryRepository = productGalleryRepository;
            _fileService = fileService;
            _productGalleryValidation = productGalleryValidation;
        }

        public async Task<ErrorOr<Success>> Handle(CreateProductGalleryCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _productGalleryValidation.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // آپلود تصویر
            string imageName = await _fileService.UploadImageAsync(request.Command.ImageFile, FileDirectories.ProductGalleryImageFolder);
            if (string.IsNullOrEmpty(imageName))
            {
                return Error.Failure(
                    code: nameof(request.Command.ImageFile),
                    description: "خطا در آپلود تصویر گالری");
            }

            try
            {
                // تغییر سایز تصویر
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductGalleryImageFolder, 100);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ProductGalleryImageFolder, 500);

                // ایجاد گالری محصول
                var gallery = new ProductGallery(request.Command.ProductId, imageName, request.Command.ImageAlt);
                await _productGalleryRepository.AddAsync(gallery);

                if (!await _productGalleryRepository.SaveChangesAsync(cancellationToken))
                {
                    await DeleteGalleryImages(imageName);
                    return Error.Failure(
                        code: "Gallery.Create",
                        description: "خطا در ایجاد تصویر گالری");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                // پاک کردن تصاویر در صورت خطا
                await DeleteGalleryImages(imageName);
                return Error.Failure(
                    code: "Gallery.Exception",
                    description: $"خطای سیستمی: {ex.Message}");
            }
        }

        private async Task DeleteGalleryImages(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductGalleryImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductGalleryImageDirectory100}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ProductGalleryImageDirectory500}{imageName}");
            }
        }
    }
}
