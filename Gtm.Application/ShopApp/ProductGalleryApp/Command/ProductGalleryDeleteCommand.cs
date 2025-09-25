using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.ProductGalleryApp.Command
{
    public record ProductGalleryDeleteCommand(int id) : IRequest<ErrorOr<Success>>;

    public class ProductGalleryDeleteCommandHandler : IRequestHandler<ProductGalleryDeleteCommand, ErrorOr<Success>>
    {
        private readonly IProductGalleryRepository _productGalleryRepository;
        private readonly IFileService _fileService;
        private readonly IProductGalleryValidation _productGalleryValidation;

        public ProductGalleryDeleteCommandHandler(IProductGalleryRepository productGalleryRepository,IFileService fileService,IProductGalleryValidation productGalleryValidation)
        {
            _productGalleryRepository = productGalleryRepository;
            _fileService = fileService;
            _productGalleryValidation = productGalleryValidation;
        }

        public async Task<ErrorOr<Success>> Handle(ProductGalleryDeleteCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _productGalleryValidation.ValidateDeleteAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // دریافت گالری (با اطمینان از وجود آن)
            var gallery = await _productGalleryRepository.GetByIdAsync(request.id);
            string imageName = gallery.ImageName;

            try
            {
                // حذف از repository
                _productGalleryRepository.Remove(gallery);

                // ذخیره تغییرات در دیتابیس
                if (await _productGalleryRepository.SaveChangesAsync(cancellationToken))
                {
                    // حذف فایل‌های تصویر فقط اگر حذف از دیتابیس موفق بود
                    await DeleteGalleryImages(imageName);
                    return Result.Success;
                }
                else
                {
                    return Error.Failure(
                        code: "ProductGallery.DeleteFailed",
                        description: "عملیات حذف تصویر گالری با خطا مواجه شد");
                }
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "ProductGallery.DeleteException",
                    description: $"خطای سیستمی در حذف تصویر گالری: {ex.Message}");
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
