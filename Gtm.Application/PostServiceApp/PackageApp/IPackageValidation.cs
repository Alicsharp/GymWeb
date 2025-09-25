using ErrorOr;
using Gtm.Contract.PostContract.UserPostContract.Command;
using static Gtm.Application.PostServiceApp.PackageApp.PackageRepoValidation;
using Utility.Appliation.FileService;

namespace Gtm.Application.PostServiceApp.PackageApp
{
    public interface IPackageValidation  
    {
        Task<ErrorOr<Success>> ValidatePackageForActivation(int packageId);
        Task<ErrorOr<Success>> ValidateCreatePackage(CreatePackage model);
        Task<ErrorOr<Success>> ValidateEditPackage(EditPackage model, int packageId);
        Task<ErrorOr<Success>> ValidateGetForEditPackage(int packageId);  
    }
    public class PackageRepoValidation : IPackageValidation
    {
        private readonly IPackageRepo _packageRepo;

        public PackageRepoValidation(IPackageRepo packageRepo)
        {
            _packageRepo = packageRepo;
        }

        public async Task<ErrorOr<Success>> ValidatePackageForActivation(int packageId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پکیج
            if (packageId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Package.Id.Invalid",
                    description: "شناسه پکیج نامعتبر است"));
            }

            // بررسی وجود پکیج
            var packageExists = await _packageRepo.ExistsAsync(p => p.Id == packageId);
            if (!packageExists)
            {
                errors.Add(Error.NotFound(
                    code: "Package.NotFound",
                    description: "پکیج مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateCreatePackage(CreatePackage model)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان تکراری
            if (await _packageRepo.ExistsAsync(p =>
                p.Title.Trim() == model.Title.Trim()))
            {
                errors.Add(Error.Conflict(
                    code: "Package.DuplicateTitle",
                    description: "عنوان پکیج نمی‌تواند تکراری باشد"));
            }

            // اعتبارسنجی تصویر
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                errors.Add(Error.Validation(
                    code: "Package.ImageRequired",
                    description: "تصویر پکیج الزامی است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditPackage(EditPackage model, int packageId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی وجود پکیج
            var package = await _packageRepo.GetByIdAsync(packageId);
            if (package == null)
            {
                errors.Add(Error.NotFound(
                    code: "Package.NotFound",
                    description: "پکیج مورد نظر یافت نشد"));
            }

            // اعتبارسنجی عنوان تکراری
            if (await _packageRepo.ExistsAsync(p =>
                p.Title.Trim() == model.Title.Trim() && p.Id != packageId))
            {
                errors.Add(Error.Conflict(
                    code: "Package.DuplicateTitle",
                    description: "عنوان پکیج نمی‌تواند تکراری باشد"));
            }

            // اعتبارسنجی تصویر
            if (model.ImageFile != null && model.ImageFile.Length == 0)
            {
                errors.Add(Error.Validation(
                    code: "Package.InvalidImage",
                    description: "تصویر پکیج نامعتبر است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditPackage(int packageId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پکیج
            if (packageId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Package.Id.Invalid",
                    description: "شناسه بسته نامعتبر است"));
            }

            // بررسی وجود پکیج
            var package = await _packageRepo.GetByIdAsync(packageId);
            if (package == null)
            {
                errors.Add(Error.NotFound(
                    code: "Package.NotFound",
                    description: "بسته مورد نظر یافت نشد"));
            }
            else
            {
                // اعتبارسنجی فیلدهای ضروری
                if (string.IsNullOrWhiteSpace(package.Title))
                {
                    errors.Add(Error.Validation(
                        code: "Package.EmptyTitle",
                        description: "عنوان بسته نمی‌تواند خالی باشد"));
                }

                if (package.Count <= 0)
                {
                    errors.Add(Error.Validation(
                        code: "Package.InvalidCount",
                        description: "تعداد بسته باید بیشتر از صفر باشد"));
                }

                if (package.Price <= 0)
                {
                    errors.Add(Error.Validation(
                        code: "Package.InvalidPrice",
                        description: "قیمت بسته باید بیشتر از صفر باشد"));
                }
            }

            return errors.Any() ? errors : Result.Success;
        }

    }
}
