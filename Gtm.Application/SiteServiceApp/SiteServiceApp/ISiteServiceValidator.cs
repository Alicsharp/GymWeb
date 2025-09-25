using ErrorOr;
using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using Utility.Appliation;
namespace Gtm.Application.SiteServiceApp.SiteServiceApp
{
    public interface ISiteServiceValidator
    {
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateSiteService command);
        Task<ErrorOr<Success>> ValidateEditAsync(EditSiteService command);
        Task<ErrorOr<Success>> ValidateGetForEditAsync(int id);
    }
    public class SiteServiceValidator : ISiteServiceValidator
    {
        private readonly ISiteServiceRepository _siteServiceRepository;

        public SiteServiceValidator(ISiteServiceRepository siteServiceRepository)
        {
            _siteServiceRepository = siteServiceRepository;
        }

        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    "SiteService.InvalidId",
                    "شناسه سرویس باید بزرگتر از صفر باشد."));
            }
            else if (!await _siteServiceRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound(
                    "SiteService.NotFound",
                    "سرویس با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateSiteService command)
        {
            var errors = new List<Error>();

            // Title validation
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    "SiteService.TitleRequired",
                    "عنوان سرویس الزامی است."));
            }
            else if (command.Title.Length > 100)
            {
                errors.Add(Error.Validation(
                    "SiteService.TitleTooLong",
                    "عنوان سرویس نباید بیشتر از 100 کاراکتر باشد."));
            }

            // Image validation
            if (command.ImageFile == null)
            {
                errors.Add(Error.Validation(
                    "SiteService.ImageRequired",
                    "تصویر سرویس الزامی است."));
            }
            else
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation(
                        "SiteService.InvalidImageType",
                        "فایل باید یک تصویر معتبر باشد."));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation(
                        "SiteService.ImageTooLarge",
                        "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }
            }

            // ImageAlt validation
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation(
                    "SiteService.ImageAltRequired",
                    "متن جایگزین تصویر الزامی است."));
            }

            // Check for duplicate title
            if (!errors.Any() && await _siteServiceRepository.ExistsAsync(s => s.Title == command.Title.Trim()))
            {
                errors.Add(Error.Conflict(
                    "SiteService.DuplicateTitle",
                    "سرویس با این عنوان قبلاً ثبت شده است."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditAsync(EditSiteService command)
        {
            var errors = new List<Error>();

            // ID validation
            if (command.Id <= 0)
            {
                errors.Add(Error.Validation(
                    "SiteService.InvalidId",
                    "شناسه سرویس معتبر نیست."));
            }

            // Title validation
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    "SiteService.TitleRequired",
                    "عنوان سرویس الزامی است."));
            }
            else if (command.Title.Length > 100)
            {
                errors.Add(Error.Validation(
                    "SiteService.TitleTooLong",
                    "عنوان سرویس نباید بیشتر از 100 کاراکتر باشد."));
            }

            // Image validation when new image is provided
            if (command.ImageFile != null)
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation(
                        "SiteService.InvalidImageType",
                        "فایل باید یک تصویر معتبر باشد."));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation(
                        "SiteService.ImageTooLarge",
                        "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }

                if (string.IsNullOrWhiteSpace(command.ImageAlt))
                {
                    errors.Add(Error.Validation(
                        "SiteService.ImageAltRequired",
                        "متن جایگزین تصویر الزامی است."));
                }
            }

            // Check for duplicate title (excluding current service)
            if (!errors.Any() && await _siteServiceRepository.ExistsAsync(s =>
                s.Title == command.Title.Trim() && s.Id != command.Id))
            {
                errors.Add(Error.Conflict(
                    "SiteService.DuplicateTitle",
                    "سرویس با این عنوان قبلاً ثبت شده است."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    "SiteService.InvalidId",
                    "شناسه سرویس باید بزرگتر از صفر باشد."));
            }
            else if (!await _siteServiceRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound(
                    "SiteService.NotFound",
                    "سرویس با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
