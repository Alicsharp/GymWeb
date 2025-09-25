using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ProductFeautreContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductFeatureApp
{
    public interface IProductFeatureValidation
    {
        Task<ErrorOr<Success>> ValidateGetFeaturesForAdminAsync(int productId);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateProductFeautre command);
        Task<ErrorOr<Success>> ValidateDeleteAsync(int featureId);
    }
    public class ProductFeatureValidationService : IProductFeatureValidation
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductFeatureRepository _productFeatureRepository;

        public ProductFeatureValidationService(IProductRepository productRepository, IProductFeatureRepository productFeatureRepository)
        {
            _productRepository = productRepository;
            _productFeatureRepository = productFeatureRepository;
        }

        public async Task<ErrorOr<Success>> ValidateGetFeaturesForAdminAsync(int productId)
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
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateProductFeautre command)
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

            // بررسی عنوان ویژگی (نباید خالی باشد)
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Title),
                    description: "عنوان ویژگی نمی‌تواند خالی باشد"));
            }

            // بررسی مقدار ویژگی (نباید خالی باشد)
            if (string.IsNullOrWhiteSpace(command.Value))
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Value),
                    description: "مقدار ویژگی نمی‌تواند خالی باشد"));
            }

            // بررسی طول عنوان ویژگی
            if (!string.IsNullOrWhiteSpace(command.Title) && command.Title.Length > 100)
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Title),
                    description: "عنوان ویژگی نمی‌تواند بیشتر از 100 کاراکتر باشد"));
            }

            // بررسی طول مقدار ویژگی
            if (!string.IsNullOrWhiteSpace(command.Value) && command.Value.Length > 200)
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Value),
                    description: "مقدار ویژگی نمی‌تواند بیشتر از 200 کاراکتر باشد"));
            }

            // بررسی تکراری نبودن ویژگی برای همان محصول (اختیاری)
            var duplicateExists = await _productFeatureRepository.ExistsAsync(f =>
                f.ProductId == command.ProductId &&
                f.Title.Trim().ToLower() == command.Title.Trim().ToLower());

            if (duplicateExists)
            {
                errors.Add(Error.Validation(
                    code: nameof(command.Title),
                    description: "این ویژگی قبلاً برای این محصول ثبت شده است"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateDeleteAsync(int featureId)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه ویژگی
            if (featureId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "ProductFeature.InvalidId",
                    description: "شناسه ویژگی معتبر نیست"));
                return errors;
            }

            // بررسی وجود ویژگی
            var featureExists = await _productFeatureRepository.ExistsAsync(f => f.Id == featureId);
            if (!featureExists)
            {
                errors.Add(Error.NotFound(
                    code: "ProductFeature.NotFound",
                    description: "ویژگی مورد نظر یافت نشد"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
    }
}
