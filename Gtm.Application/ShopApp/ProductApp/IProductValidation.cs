using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ProductContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.Slug;

namespace Gtm.Application.ShopApp.ProductApp
{
    public interface IProductValidation
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateProduct command);
        Task<ErrorOr<Success>> ValidateGetForEditAsync(int productId);
        Task<ErrorOr<Success>> ValidateEditAsync(EditProduct command);
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int productId);
    }

    public class ProductValidatior : IProductValidation
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductValidatior(IProductRepository productRepository,IProductCategoryRepository productCategoryRepository)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateProduct command)
        {
            var errors = new List<Error>();

            // بررسی تکراری نبودن عنوان محصول
            if (await _productRepository.ExistsAsync(p => p.Title.Trim().ToLower() == command.Title.Trim().ToLower()))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Title),
                    description: "عنوان محصول تکراری است"));
            }

            // بررسی تکراری نبودن slug
            var slug = SlugUtility.GenerateSlug(command.Slug);
            if (await _productRepository.ExistsAsync(p => p.Slug == slug))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Slug),
                    description: "شناسه محصول تکراری است"));
            }

            // بررسی وجود دسته‌بندی‌ها
            if (command.Categoryids.Count() < 1 || !await _productCategoryRepository.CheckProductCategoriesExist(command.Categoryids))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Categoryids),
                    description: "لطفا دسته‌بندی‌های معتبر انتخاب کنید"));
            }

            // بررسی تصویر محصول
            if (command.ImageFile == null || !command.ImageFile.IsImage())
            {
                errors.Add(Error.Validation(
                    code: nameof(command.ImageFile),
                    description: "لطفا یک تصویر معتبر انتخاب کنید"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int productId)
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
        public async Task<ErrorOr<Success>> ValidateEditAsync(EditProduct command)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه محصول
            if (command.Id <= 0)
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Id),
                    description: "شناسه محصول معتبر نیست"));
                return errors;
            }

            // بررسی وجود محصول
            var productExists = await _productRepository.ExistsAsync(p => p.Id == command.Id);
            if (!productExists)
            {
                errors.Add(Error.NotFound(
                    code: nameof(command.Id),
                    description: "محصول مورد نظر یافت نشد"));
                return errors;
            }

            // بررسی تکراری نبودن عنوان (به جز خود محصول)
            if (await _productRepository.ExistsAsync(p =>
                p.Title.Trim().ToLower() == command.Title.Trim().ToLower() &&
                p.Id != command.Id))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Title),
                    description: "عنوان محصول تکراری است"));
            }

            // بررسی تکراری نبودن slug (به جز خود محصول)
            var slug = SlugUtility.GenerateSlug(command.Slug);
            if (await _productRepository.ExistsAsync(p =>
                p.Slug == slug &&
                p.Id != command.Id))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Slug),
                    description: "شناسه محصول تکراری است"));
            }

            // بررسی وجود دسته‌بندی‌ها
            if (command.Categoryids.Count() < 1 || !await _productCategoryRepository.CheckProductCategoriesExist(command.Categoryids))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Categoryids),
                    description: "لطفا دسته‌بندی‌های معتبر انتخاب کنید"));
            }

            // بررسی تصویر محصول (اگر فایل جدید ارسال شده)
            if (command.ImageFile != null && !command.ImageFile.IsImage())
            {
                errors.Add(Error.Validation(
                    code: nameof(command.ImageFile),
                    description: "لطفا یک تصویر معتبر انتخاب کنید"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int productId)
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
    }
}
