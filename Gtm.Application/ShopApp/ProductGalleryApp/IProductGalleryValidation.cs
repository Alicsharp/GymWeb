using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ProductGalleryContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.ProductGalleryApp
{
    public interface IProductGalleryValidation
    {
        Task<ErrorOr<Success>> ValidateGetGalleriesForAdminAsync(int productId);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateProductGallery command);
        Task<ErrorOr<Success>> ValidateDeleteAsync(int galleryId);
    }
    public class ProductGalleryValidationService : IProductGalleryValidation
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductGalleryRepository _productGalleryRepository;

        public ProductGalleryValidationService(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository, IProductGalleryRepository productGalleryRepository)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productGalleryRepository = productGalleryRepository;
        }

        // سایر متدهای موجود...

        public async Task<ErrorOr<Success>> ValidateGetGalleriesForAdminAsync(int productId)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه محصول
            if (productId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Product.InvalidId",
                    description: "شناسه محصول معتبر نیست"));
                return errors;
            }

            // بررسی وجود محصول
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
            {
                errors.Add(Error.NotFound(
                    code: "Product.NotFound",
                    description: "محصول مورد نظر یافت نشد"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateProductGallery command)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه محصول
            if (command.ProductId <= 0)
            {
                errors.Add(Error.Validation(
                    code: nameof(command.ProductId),
                    description: "شناسه محصول معتبر نیست"));
                return errors;
            }

            // بررسی وجود محصول
            var productExists = await _productRepository.ExistsAsync(p => p.Id == command.ProductId);
            if (!productExists)
            {
                errors.Add(Error.Validation(
                    code: nameof(command.ProductId),
                    description: "محصول مورد نظر یافت نشد"));
                return errors;
            }

            // بررسی تصویر گالری
            if (command.ImageFile == null || !command.ImageFile.IsImage())
            {
                errors.Add(Error.Validation(
                    code: nameof(command.ImageFile),
                    description: "لطفا یک تصویر معتبر انتخاب کنید"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateDeleteAsync(int galleryId)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه گالری
            if (galleryId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "ProductGallery.InvalidId",
                    description: "شناسه گالری معتبر نیست"));
                return errors;
            }

            // بررسی وجود گالری
            var galleryExists = await _productGalleryRepository.ExistsAsync(g => g.Id == galleryId);
            if (!galleryExists)
            {
                errors.Add(Error.NotFound(
                    code: "ProductGallery.NotFound",
                    description: "تصویر گالری مورد نظر یافت نشد"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
    }
}
