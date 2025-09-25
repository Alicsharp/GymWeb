using ErrorOr;
using Gtm.Contract.SiteContract.BanarContract.Command;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.BannerApp
{
    public interface IBannerValidator
    {
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateBaner command);
        Task<ErrorOr<Success>> ValidateUpdateAsync(EditBaner command);
        Task<ErrorOr<Success>> ValidateGetForEditAsync(int id);
        ErrorOr<Success> ValidateGetForUi(int count, BanerState state);
    }
    public class BannerValidator : IBannerValidator
    {
        private readonly IBanerRepository _banerRepository;

        public BannerValidator(IBanerRepository banerRepository)
        {
            _banerRepository = banerRepository;
        }

        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
                errors.Add(Error.Validation("Banner.Id.Invalid", "شناسه بنر معتبر نیست."));

            if (!await _banerRepository.ExistsAsync(c=>c.Id==id))
                errors.Add(Error.NotFound(description: "بنر مورد نظر یافت نشد."));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateBaner command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی تصویر
            if (command.ImageFile == null)
            {
                errors.Add(Error.Validation("Banner.ImageMissing", "تصویر خالی است"));
            }
            else
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation("Banner.InvalidImage", "فایل ارسال شده باید یک تصویر معتبر باشد."));
                }

                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(command.ImageFile.FileName).ToLower();

                if (!validExtensions.Contains(fileExtension))
                {
                    errors.Add(Error.Validation("Banner.InvalidImageFormat",
                        "فرمت تصویر نامعتبر است. فقط فرمت‌های JPG, JPEG, PNG, GIF قابل قبول هستند."));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("Banner.ImageTooLarge",
                        "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }
            }

            // اعتبارسنجی ImageAlt
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation("Banner.ImageAltRequired",
                    "متن جایگزین تصویر الزامی است."));
            }

            // اعتبارسنجی URL
            if (!string.IsNullOrWhiteSpace(command.Url) &&
                !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("Banner.InvalidUrl",
                    "آدرس URL معتبر نیست."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateUpdateAsync(EditBaner command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی وجود بنر
            if (!await _banerRepository.ExistsAsync(c=>c.Id==command.Id))
            {
                errors.Add(Error.NotFound("Banner.NotFound", "بنر با این شناسه پیدا نشد."));
            }

            // اعتبارسنجی تصویر (در صورت وجود)
            if (command.ImageFile != null)
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation("Banner.InvalidImage", "فایل ارسال شده باید یک تصویر معتبر باشد."));
                }

                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(command.ImageFile.FileName).ToLower();

                if (!validExtensions.Contains(fileExtension))
                {
                    errors.Add(Error.Validation("Banner.InvalidImageFormat",
                        "فرمت تصویر نامعتبر است. فقط فرمت‌های JPG, JPEG, PNG, GIF قابل قبول هستند."));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("Banner.ImageTooLarge",
                        "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }
            }

            // اعتبارسنجی ImageAlt
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation("Banner.ImageAltRequired",
                    "متن جایگزین تصویر الزامی است."));
            }

            // اعتبارسنجی URL
            if (!string.IsNullOrWhiteSpace(command.Url) &&
                !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("Banner.InvalidUrl",
                    "آدرس URL معتبر نیست."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation("Banner.Id.Invalid", "شناسه بنر معتبر نیست."));
            }
            else if (!await _banerRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound("Banner.NotFound", "بنر مورد نظر یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public ErrorOr<Success> ValidateGetForUi(int count, BanerState state)
        {
            var errors = new List<Error>();

            if (count <= 0)
            {
                errors.Add(Error.Validation("Banner.Count.Invalid", "تعداد باید بیشتر از صفر باشد."));
            }

            if (!Enum.IsDefined(typeof(BanerState), state))
            {
                errors.Add(Error.Validation("Banner.State.Invalid", "وضعیت بنر معتبر نیست."));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
