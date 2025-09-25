using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Application.UserApp;
using Gtm.Contract.PostContract.UserPostContract.Command;

namespace Gtm.Application.PostServiceApp.UserPostApp
{
    public interface IPostOrderValidation
    {
 
        Task<ErrorOr<Success>> ValidateUserAndPackageAsync(int userId, int packageId);
        Task<ErrorOr<Success>> ValidateGetPostOrderNotPaymentAsync(int userId);
        Task<ErrorOr<Success>> ValidateCreatePostModelAsync(CreatePostOrder model);
  
    }
    public class PostOrderValidationService : IPostOrderValidation
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IUserRepo _userRepo;

        public PostOrderValidationService(IPackageRepo packageRepository, IUserRepo userRepo)
        {
            _packageRepository = packageRepository;
            _userRepo = userRepo;
        }

        

        public async Task<ErrorOr<Success>> ValidateCreatePostModelAsync(CreatePostOrder model)
        {
            if (model == null)
                return Error.Validation(
                    code: "CreatePostModel.Null",
                    description: "مدل ایجاد پست نمی‌تواند خالی باشد");

            var errors = new List<Error>();

            // اعتبارسنجی همزمان
            if (model.Price <= 0)
            {
                errors.Add(Error.Validation(
                    code: "CreatePostModel.InvalidPrice",
                    description: "مبلغ پکیج باید بزرگتر از صفر باشد"));
            }

            // اعتبارسنجی غیرهمزمان
            if (model.PackageId > 0)
            {
                var packageValidation = await ValidatePackagePriceAsync(model.PackageId, model.Price);
                if (packageValidation.IsError)
                    errors.AddRange(packageValidation.Errors);
            }

            return errors.Any() ? errors : Result.Success;
        }

        private async Task<ErrorOr<Success>> CheckUserExistsAsync(int userId)
        {
            var exists = await _userRepo.ExistsAsync(C=>C.Id==userId);
            return exists
                ? Result.Success
                : Error.NotFound("User.NotFound", "کاربر مورد نظر یافت نشد");
        }

        private async Task<ErrorOr<Success>> CheckPackageExistsAsync(int packageId)
        {
            var exists = await _userRepo.ExistsAsync(C => C.Id == packageId);
            return exists
                ? Result.Success
                : Error.NotFound("Package.NotFound", "پکیج مورد نظر یافت نشد");
        }

        private async Task<ErrorOr<Success>> ValidatePackagePriceAsync(int packageId, decimal price)
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            if (package?.Price != price)
            {
                return Error.Validation(
                    code: "CreatePostModel.PriceMismatch",
                    description: "مبلغ پکیج با مبلغ سیستم مطابقت ندارد");
            }
            return Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateUserAndPackageAsync(int userId, int packageId)
        {
            var errors = new List<Error>();

            if (userId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "User.InvalidId",
                    description: "شناسه کاربر نامعتبر است"));
            }

            if (packageId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Package.InvalidId",
                    description: "شناسه پکیج نامعتبر است"));
            }

            if (errors.Any())
            {
                return errors;
            }

            var packageExists = await _packageRepository.ExistsAsync(c=>c.Id==packageId);
            if (!packageExists)
            {
                return Error.NotFound(
                    code: "Package.NotFound",
                    description: "پکیج مورد نظر یافت نشد");
            }

            return Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetPostOrderNotPaymentAsync(int userId)
        {
            var errors = new List<Error>();

            if (userId <= 0)
                errors.Add(Error.Validation("PostOrder.InvalidUserId", "شناسه کاربر نامعتبر است"));

            // بررسی وجود کاربر (اگر نیاز است)
            // if (!await _userRepository.ExistsAsync(u => u.Id == userId))
            //     errors.Add(Error.NotFound("User.NotFound", "کاربر یافت نشد"));

            return errors.Any() ? errors : Result.Success;
        }
    }
}
