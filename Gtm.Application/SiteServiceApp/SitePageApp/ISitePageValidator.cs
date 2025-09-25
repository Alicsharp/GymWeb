using ErrorOr;
using Gtm.Contract.SiteContract.SitePageContract.Command;
using Utility.Appliation.Slug;
namespace Gtm.Application.SiteServiceApp.SitePageApp
{
    public interface ISitePageValidator
    {
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateSitePage command);
        Task<ErrorOr<Success>> ValidateEditAsync(EditSitePage command);
        Task<ErrorOr<Success>> ValidateGetForEditAsync(int id);
        Task<ErrorOr<Success>> ValidateGetBySlugAsync(string slug);
    }
    public class SitePageValidator : ISitePageValidator
    {
        private readonly ISitePageRepository _sitePageRepository;

        public SitePageValidator(ISitePageRepository sitePageRepository)
        {
            _sitePageRepository = sitePageRepository;
        }

        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    "SitePage.InvalidId",
                    "شناسه صفحه باید بزرگتر از صفر باشد."));
            }
            else if (!await _sitePageRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound(
                    "SitePage.NotFound",
                    "صفحه با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateSitePage command)
        {
            var errors = new List<Error>();

            // Title validation
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    "SitePage.TitleRequired",
                    "عنوان صفحه الزامی است."));
            }
            else if (command.Title.Length > 100)
            {
                errors.Add(Error.Validation(
                    "SitePage.TitleTooLong",
                    "عنوان صفحه نباید بیشتر از 100 کاراکتر باشد."));
            }

            // Slug validation
            var slug = command.Slug.GenerateSlug();
            if (string.IsNullOrWhiteSpace(slug))
            {
                errors.Add(Error.Validation(
                    "SitePage.InvalidSlug",
                    "Slug معتبر نیست."));
            }
            else if (slug.Length > 150)
            {
                errors.Add(Error.Validation(
                    "SitePage.SlugTooLong",
                    "Slug نباید بیشتر از 150 کاراکتر باشد."));
            }

            // Content validation
            if (string.IsNullOrWhiteSpace(command.Text))
            {
                errors.Add(Error.Validation(
                    "SitePage.ContentRequired",
                    "محتویات صفحه نمی‌تواند خالی باشد."));
            }

            // Check for duplicates (only if no previous errors)
            if (!errors.Any())
            {
                var trimmedTitle = command.Title.Trim();
                if (await _sitePageRepository.ExistsAsync(c => c.Title == trimmedTitle))
                {
                    errors.Add(Error.Conflict(
                        "SitePage.DuplicateTitle",
                        "عنوان صفحه تکراری است."));
                }

                if (await _sitePageRepository.ExistsAsync(c => c.Slug == slug))
                {
                    errors.Add(Error.Conflict(
                        "SitePage.DuplicateSlug",
                        "Slug تکراری است."));
                }
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditAsync(EditSitePage command)
        {
            var errors = new List<Error>();

            // ID validation
            if (command.Id <= 0)
            {
                errors.Add(Error.Validation(
                    "SitePage.InvalidId",
                    "شناسه صفحه معتبر نیست."));
            }

            // Title validation
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    "SitePage.TitleRequired",
                    "عنوان صفحه الزامی است."));
            }
            else if (command.Title.Trim().Length > 100)
            {
                errors.Add(Error.Validation(
                    "SitePage.TitleTooLong",
                    "عنوان صفحه نباید بیشتر از 100 کاراکتر باشد."));
            }

            // Slug validation
            var slug = command.Slug.GenerateSlug();
            if (string.IsNullOrWhiteSpace(slug))
            {
                errors.Add(Error.Validation(
                    "SitePage.InvalidSlug",
                    "Slug معتبر نیست."));
            }
            else if (slug.Length > 150)
            {
                errors.Add(Error.Validation(
                    "SitePage.SlugTooLong",
                    "Slug نباید بیشتر از 150 کاراکتر باشد."));
            }

            // Content validation
            if (string.IsNullOrWhiteSpace(command.Text))
            {
                errors.Add(Error.Validation(
                    "SitePage.ContentRequired",
                    "محتویات صفحه نمی‌تواند خالی باشد."));
            }

            // Check for duplicates (only if no previous errors)
            if (!errors.Any())
            {
                var trimmedTitle = command.Title.Trim();
                if (await _sitePageRepository.ExistsAsync(c =>
                    c.Title == trimmedTitle && c.Id != command.Id))
                {
                    errors.Add(Error.Conflict(
                        "SitePage.DuplicateTitle",
                        "عنوان صفحه تکراری است."));
                }

                if (await _sitePageRepository.ExistsAsync(c =>
                    c.Slug == slug && c.Id != command.Id))
                {
                    errors.Add(Error.Conflict(
                        "SitePage.DuplicateSlug",
                        "Slug تکراری است."));
                }
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    "SitePage.InvalidId",
                    "شناسه صفحه باید بزرگتر از صفر باشد."));
            }
            else if (!await _sitePageRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound(
                    "SitePage.NotFound",
                    "صفحه با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetBySlugAsync(string slug)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(slug))
            {
                errors.Add(Error.Validation(
                    "SitePage.InvalidSlug",
                    "اسلاگ صفحه نمی‌تواند خالی باشد."));
            }
            else if (slug.Length > 100)
            {
                errors.Add(Error.Validation(
                    "SitePage.SlugTooLong",
                    "اسلاگ صفحه نمی‌تواند بیشتر از 100 کاراکتر باشد."));
            }
            else if (!await _sitePageRepository.ExistsAsync(p => p.Slug == slug))
            {
                errors.Add(Error.NotFound(
                    "SitePage.NotFound",
                    "صفحه با این اسلاگ یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
