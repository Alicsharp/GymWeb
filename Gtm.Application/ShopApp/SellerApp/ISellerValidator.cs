using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Application.ShopApp.SellerApp.Query;
using Gtm.Application.UserApp;
using Gtm.Contract.SellerContract.Command;
using Utility.Appliation;
using Utility.Domain.Enums;


namespace Gtm.Application.ShopApp.SellerApp
{
    public interface ISellerValidator
    {
        Task<ErrorOr<Success>> ValidateGetSellersForUserQueryAsync(int userId  );
        Task<ErrorOr<Success>> ValidateGetSellersForAdminQuery(int pageId, int take, string filter);
        Task<ErrorOr<Success>> ValidateRequestSellerAsync(int userId,RequestSeller command);
        Task<ErrorOr<Success>> ValidateId(int id);
        Task<ErrorOr<Success>> GetSellerForEditValidator(int id, int userId);
        Task<ErrorOr<Success>> EditSellerValidation(int userId, EditSellerRequest command);
        Task<ErrorOr<Success>> ValidateGetSellerDetailForSeller(GetSellerDetailForSellerQuery query);

    }
    public class SellerValidator : ISellerValidator
    {
        private readonly IUserRepo _userRepository;
        private readonly ISellerRepository _sellerRepository;

        public SellerValidator(IUserRepo userRepository, ISellerRepository sellerRepository)
        {
            _userRepository = userRepository;
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<Success>> ValidateId(int id)
        {
            var errors = new List<Error>();

            // اعتبارسنجی id
            if (id <= 0)
                errors.Add(Error.Validation("IdInvalid", "شناسه باید بزرگتر از صفر باشد"));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetSellersForUserQueryAsync(int userId   )
        {
            var errors = new List<Error>();

            // اعتبارسنجی userId
            if (  userId <= 0)
            {
                errors.Add(Error.Validation("Seller.UserIdInvalid", "شناسه کاربر نامعتبر است."));
            }
            else
            {
                // بررسی وجود کاربر
                var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
                if (!userExists)
                {
                    errors.Add(Error.Validation("Seller.UserNotFound", "کاربر مورد نظر یافت نشد."));
                }
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateGetSellersForAdminQuery(int pageId, int take, string filter)
        {
            var errors = new List<Error>();

            // اعتبارسنجی pageId
            if (pageId <= 0)
                errors.Add(Error.Validation("PageIdInvalid", "شماره صفحه باید بزرگتر از صفر باشد"));

            // اعتبارسنجی take
            if (take <= 0)
                errors.Add(Error.Validation("TakeInvalid", "تعداد آیتم‌ها باید بزرگتر از صفر باشد"));

            if (take > 100)
                errors.Add(Error.Validation("TakeTooLarge", "تعداد آیتم‌ها نمی‌تواند بیشتر از ۱۰۰ باشد"));

            // اعتبارسنجی filter (اختیاری)
            if (!string.IsNullOrEmpty(filter) && filter.Length < 2)
                errors.Add(Error.Validation("FilterTooShort", "فیلتر جستجو باید حداقل ۲ کاراکتر باشد"));

            if (!string.IsNullOrEmpty(filter) && filter.Length > 100)
                errors.Add(Error.Validation("FilterTooLong", "فیلتر جستجو نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد"));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateRequestSellerAsync(int userId,  RequestSeller command)
        {
            var errors = new List<Error>();
            if (userId <= 0)
                errors.Add(Error.Validation(
                         code: "User.NotExist",
                         description: ValidationMessages.UserNotFound));
            // اعتبارسنجی تصویر اصلی
            if (command.ImageFile == null || !command.ImageFile.IsImage())
            {
                errors.Add(Error.Validation(
                    code: "Seller.ImageFile",
                    description: ValidationMessages.ImageErrorMessage));
            }

            // اعتبارسنجی تصویر پذیرش
            if (command.ImageAccept == null || !command.ImageAccept.IsImage())
            {
                errors.Add(Error.Validation(
                    code: "Seller.ImageAccept",
                    description: ValidationMessages.ImageErrorMessage));
            }

            // عنوان فروشنده
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    code: "Seller.TitleRequired",
                    description: "عنوان فروشگاه الزامی است"));
            }
            else if (command.Title.Length > 150)
            {
                errors.Add(Error.Validation(
                    code: "Seller.TitleTooLong",
                    description: "عنوان فروشگاه نمی‌تواند بیشتر از 150 کاراکتر باشد"));
            }

            // استان و شهر
            if (command.StateId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Seller.StateRequired",
                    description: "انتخاب استان الزامی است"));
            }

            if (command.CityId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Seller.CityRequired",
                    description: "انتخاب شهر الزامی است"));
            }

            // آدرس
            if (string.IsNullOrWhiteSpace(command.Address))
            {
                errors.Add(Error.Validation(
                    code: "Seller.AddressRequired",
                    description: "آدرس فروشگاه الزامی است"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> GetSellerForEditValidator(int id, int userId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی Id
            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Seller.InvalidId",
                    description: "شناسه فروشنده نامعتبر است"));
            }

            // اعتبارسنجی UserId
            if (userId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Seller.InvalidUser",
                    description: "کاربر نامعتبر است"));
            }

            if (errors.Any())
                return errors;

            // گرفتن Seller
            var seller = await _sellerRepository.GetByIdAsync(id);
            if (seller == null)
            {
                return Error.NotFound("Seller.NotFound", "فروشنده یافت نشد");
            }

            if (seller.UserId != userId)
            {
                return Error.Validation(
                    code: "Seller.AccessDenied",
                    description: "شما به این فروشنده دسترسی ندارید");
            }

            if (seller.Status != SellerStatus.درخواست_تایید_نشده)
            {
                return Error.Validation(
                    code: "Seller.InvalidStatus",
                    description: "امکان ویرایش این فروشنده وجود ندارد");
            }

            return Result.Success;
        }

        public async Task<ErrorOr<Success>> EditSellerValidation(int userId, EditSellerRequest command)
        {
            var errors = new List<Error>();

            // 1. چک کردن مقادیر پایه
            if (userId <= 0)
            {
                errors.Add(Error.Validation("Seller.InvalidUser", "کاربر نامعتبر است"));
            }

            if (command.Id <= 0)
            {
                errors.Add(Error.Validation("Seller.InvalidId", "شناسه فروشنده نامعتبر است"));
            }

            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation("Seller.TitleRequired", "عنوان فروشگاه الزامی است"));
            }

            if (command.StateId <= 0)
            {
                errors.Add(Error.Validation("Seller.StateRequired", "استان انتخاب نشده است"));
            }

            if (command.CityId <= 0)
            {
                errors.Add(Error.Validation("Seller.CityRequired", "شهر انتخاب نشده است"));
            }

            if (string.IsNullOrWhiteSpace(command.Address))
            {
                errors.Add(Error.Validation("Seller.AddressRequired", "آدرس فروشگاه الزامی است"));
            }

            if (errors.Any()) return errors;

            // 2. بررسی موجودیت فروشنده
            var seller = await _sellerRepository.GetByIdAsync(command.Id);
            if (seller == null || seller.UserId != userId)
            {
                return Error.NotFound("Seller.NotFound", "فروشنده پیدا نشد یا متعلق به شما نیست");
            }

            // 3. بررسی فایل‌ها
            if (command.ImageFile != null && !command.ImageFile.IsImage())
            {
                return Error.Validation("Seller.ImageFile", ValidationMessages.ImageErrorMessage);
            }

            if (command.ImageAccept != null && !command.ImageAccept.IsImage())
            {
                return Error.Validation("Seller.ImageAccept", ValidationMessages.ImageErrorMessage);
            }

            return Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateGetSellerDetailForSeller(GetSellerDetailForSellerQuery query)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه فروشنده
            if (query.id <= 0)
            {
                errors.Add(Error.Validation("Seller.Id.Invalid", "شناسه فروشنده باید بزرگتر از صفر باشد."));
            }

            // اعتبارسنجی شناسه کاربر
            if (query.userId <= 0)
            {
                errors.Add(Error.Validation("User.Id.Invalid", "شناسه کاربر باید بزرگتر از صفر باشد."));
            }

            // اگر خطای اولیه وجود دارد، ادامه نده
            if (errors.Any())
            {
                return errors;
            }

            // بررسی وجود فروشنده
            var sellerExists = await _sellerRepository.ExistsAsync(s => s.Id == query.id && s.UserId == query.userId);
            if (!sellerExists)
            {
                errors.Add(Error.NotFound(description: "فروشنده یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}

