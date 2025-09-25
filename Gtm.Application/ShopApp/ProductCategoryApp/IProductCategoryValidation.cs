using ErrorOr;
using Gtm.Contract.ProductCategoryContract.Command;
using Utility.Appliation.Slug;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.ProductCategoryApp
{
    public interface IProductCategoryValidation
    {
        Task<ErrorOr<Success>> ValidateCategoryHasParentAsync(int categoryId);
        Task<ErrorOr<Success>> ValidateCategoryExistenceAsync(int categoryId);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateProductCategory command);
        Task<ErrorOr<Success>> ValidateGetForEditAsync(int categoryId);
        Task<ErrorOr<Success>> ValidateEditAsync(EditProductCategory command);
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int categoryId);
    }
    public class ProductCategoryValidationService : IProductCategoryValidation
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductCategoryValidationService(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCategoryExistsAsync(int categoryId)
        {
            var errors = new List<Error>();

            var category = await _productCategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                errors.Add(Error.Validation("ProductCategory.NotFound", "دسته‌بندی محصول یافت نشد."));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateCategoryExistenceAsync(int categoryId)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه (باید بزرگتر از صفر باشد)
            if (categoryId < 0)
            {
                errors.Add(Error.Validation("ProductCategory.InvalidId", "شناسه دسته‌بندی معتبر نیست."));
            }

            // اگر شناسه بزرگتر از صفر است، وجود دسته‌بندی را بررسی می‌کنیم
            if (categoryId > 0)
            {
                var category = await _productCategoryRepository.GetByIdAsync(categoryId);
                if (category == null)
                {
                    errors.Add(Error.Validation("ProductCategory.NotFound", "دسته‌بندی مورد نظر یافت نشد."));
                }
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCategoryHasParentAsync(int categoryId)
        {
            var errors = new List<Error>();

            // ابتدا وجود دسته‌بندی را بررسی می‌کنیم
            var existsResult = await ValidateCategoryExistsAsync(categoryId);
            if (existsResult.IsError)
            {
                return existsResult.Errors;
            }

            // بررسی داشتن والد
            var category = await _productCategoryRepository.GetByIdAsync(categoryId);
            if (category != null && category.Parent <= 0)
            {
                errors.Add(Error.Validation("ProductCategory.NoParent", "دسته‌بندی والد ندارد."));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateCategoryIdAsync(int categoryId)
        {
            var errors = new List<Error>();

            if (categoryId <= 0)
            {
                errors.Add(Error.Validation("ProductCategory.InvalidId", "شناسه دسته‌بندی معتبر نیست."));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateProductCategory command)
        {
            var errors = new List<Error>();

            // بررسی عنوان
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(nameof(command.Title), "عنوان دسته‌بندی الزامی است."));
            }
            else if (command.Title.Trim().Length > 150)
            {
                errors.Add(Error.Validation(nameof(command.Title), "عنوان دسته‌بندی نمی‌تواند بیشتر از 150 کاراکتر باشد."));
            }
            else
            {
                // بررسی تکراری نبودن عنوان
                if (await _productCategoryRepository.ExistsAsync(p =>
                    p.Title.Trim().ToLower() == command.Title.Trim().ToLower()))
                {
                    errors.Add(Error.Validation(nameof(command.Title), "عنوان دسته‌بندی تکراری است."));
                }
            }

            // بررسی slug
            if (string.IsNullOrWhiteSpace(command.Slug))
            {
                errors.Add(Error.Validation(nameof(command.Slug), "شناسه دسته‌بندی الزامی است."));
            }
            else
            {
                var slug = SlugUtility.GenerateSlug(command.Slug);
                if (await _productCategoryRepository.ExistsAsync(p => p.Slug == slug))
                {
                    errors.Add(Error.Validation(nameof(command.Slug), "شناسه دسته‌بندی تکراری است."));
                }
            }

            // بررسی تصویر
            if (command.ImageFile == null)
            {
                errors.Add(Error.Validation(nameof(command.ImageFile), "تصویر دسته‌بندی الزامی است."));
            }
            else if (!command.ImageFile.IsImage())
            {
                errors.Add(Error.Validation(nameof(command.ImageFile), "لطفا یک تصویر معتبر انتخاب کنید."));
            }
            else if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
            {
                errors.Add(Error.Validation(nameof(command.ImageFile), "حجم تصویر نمی‌تواند بیشتر از 5 مگابایت باشد."));
            }

            // بررسی imageAlt
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation(nameof(command.ImageAlt), "متن جایگزین تصویر الزامی است."));
            }
            else if (command.ImageAlt.Length > 200)
            {
                errors.Add(Error.Validation(nameof(command.ImageAlt), "متن جایگزین تصویر نمی‌تواند بیشتر از 200 کاراکتر باشد."));
            }

            // بررسی parent
            if (command.Parent > 0)
            {
                var parentCategory = await _productCategoryRepository.GetByIdAsync(command.Parent);
                if (parentCategory == null)
                {
                    errors.Add(Error.Validation(nameof(command.Parent), "دسته‌بندی والد معتبر نیست."));
                }
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int categoryId)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه
            if (categoryId <= 0)
            {
                errors.Add(Error.Validation("ProductCategory.InvalidId", "شناسه دسته‌بندی معتبر نیست."));
                return errors;
            }

            // بررسی وجود دسته‌بندی
            var category = await _productCategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                errors.Add(Error.Validation("ProductCategory.NotFound", "دسته‌بندی مورد نظر یافت نشد."));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditAsync(EditProductCategory command)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه
            if (command.Id <= 0)
            {
                errors.Add(Error.Validation("ProductCategory.InvalidId", "شناسه دسته‌بندی معتبر نیست."));
                return errors;
            }

            // بررسی وجود دسته‌بندی
            var category = await _productCategoryRepository.GetByIdAsync(command.Id);
            if (category == null)
            {
                errors.Add(Error.Validation("ProductCategory.NotFound", "دسته‌بندی مورد نظر یافت نشد."));
                return errors;
            }

            // بررسی عنوان
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(nameof(command.Title), "عنوان دسته‌بندی الزامی است."));
            }
            else if (command.Title.Trim().Length > 150)
            {
                errors.Add(Error.Validation(nameof(command.Title), "عنوان دسته‌بندی نمی‌تواند بیشتر از 150 کاراکتر باشد."));
            }
            else
            {
                // بررسی تکراری نبودن عنوان
                if (await _productCategoryRepository.ExistsAsync(p =>
                    p.Title.Trim().ToLower() == command.Title.Trim().ToLower() &&
                    p.Id != command.Id))
                {
                    errors.Add(Error.Validation(nameof(command.Title), "عنوان دسته‌بندی تکراری است."));
                }
            }

            // بررسی slug
            if (string.IsNullOrWhiteSpace(command.Slug))
            {
                errors.Add(Error.Validation(nameof(command.Slug), "شناسه دسته‌بندی الزامی است."));
            }
            else
            {
                var slug = SlugUtility.GenerateSlug(command.Slug);
                if (await _productCategoryRepository.ExistsAsync(p =>
                    p.Slug == slug &&
                    p.Id != command.Id))
                {
                    errors.Add(Error.Validation(nameof(command.Slug), "شناسه دسته‌بندی تکراری است."));
                }
            }

            // بررسی imageAlt
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation(nameof(command.ImageAlt), "متن جایگزین تصویر الزامی است."));
            }
            else if (command.ImageAlt.Length > 200)
            {
                errors.Add(Error.Validation(nameof(command.ImageAlt), "متن جایگزین تصویر نمی‌تواند بیشتر از 200 کاراکتر باشد."));
            }

            // بررسی تصویر (اگر ارسال شده)
            if (command.ImageFile != null)
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation(nameof(command.ImageFile), "لطفا یک تصویر معتبر انتخاب کنید."));
                }
                else if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation(nameof(command.ImageFile), "حجم تصویر نمی‌تواند بیشتر از 5 مگابایت باشد."));
                }
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int categoryId)
        {
            var errors = new List<Error>();

            // بررسی معتبر بودن شناسه
            if (categoryId <= 0)
            {
                errors.Add(Error.Validation("ProductCategory.InvalidId", "شناسه دسته‌بندی معتبر نیست."));
                return errors;
            }

            // بررسی وجود دسته‌بندی
            var category = await _productCategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                errors.Add(Error.Validation("ProductCategory.NotFound", "دسته‌بندی مورد نظر یافت نشد."));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        // سایر متدها...
    }

}


 
 
 


