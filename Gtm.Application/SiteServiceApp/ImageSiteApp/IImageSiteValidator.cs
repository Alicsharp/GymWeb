using ErrorOr;
using Gtm.Contract.SiteContract.ImageSiteContract.Command;
using Utility.Appliation.FileService;
using Utility.Appliation;

namespace Gtm.Application.SiteServiceApp.ImageSiteApp
{
    public interface IImageSiteValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateImageSite command);
        Task<ErrorOr<Success>> ValidateDeleteAsync(int id);
        ErrorOr<Success> ValidateGetAllForAdmin(int pageId, int take, string filter);
    }
    public class ImageSiteValidator : IImageSiteValidator
    {
        private readonly IFileService _fileService;
        private readonly IImageSiteRepository _imageSiteRepository;

        public ImageSiteValidator(IFileService fileService,IImageSiteRepository imageSiteRepository)
        {
            _fileService = fileService;
            _imageSiteRepository = imageSiteRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateImageSite command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی تصویر
            if (command.ImageFile == null)
            {
                errors.Add(Error.Validation("ImageSite.ImageRequired", "تصویر الزامی است."));
            }
            else
            {
                // بررسی نوع فایل
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation("ImageSite.InvalidImageType", "فایل باید یک تصویر معتبر باشد."));
                }

                // بررسی حجم فایل
                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("ImageSite.ImageTooLarge", "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }
            }

            // اعتبارسنجی عنوان
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation("ImageSite.TitleRequired", "عنوان الزامی است."));
            }
            else if (command.Title.Length > 100)
            {
                errors.Add(Error.Validation("ImageSite.TitleTooLong", "عنوان نباید بیشتر از 100 کاراکتر باشد."));
            }
            else
            {
                // بررسی یکتا بودن عنوان
                var titleUniquenessResult = await ValidateTitleUniquenessAsync(command.Title);
                if (titleUniquenessResult.IsError)
                {
                    errors.AddRange(titleUniquenessResult.Errors);
                }
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateDeleteAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation("ImageSite.IdInvalid", "شناسه تصویر معتبر نیست."));
            }
            else if (!await _imageSiteRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound("ImageSite.NotFound", "تصویر با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public ErrorOr<Success> ValidateGetAllForAdmin(int pageId, int take, string filter)
        {
            var errors = new List<Error>();

            if (pageId < 1)
            {
                errors.Add(Error.Validation("Pagination.PageInvalid", "شماره صفحه باید بزرگتر از 0 باشد."));
            }

            if (take < 1 || take > 100)
            {
                errors.Add(Error.Validation("Pagination.TakeInvalid", "تعداد آیتم‌ها باید بین 1 تا 100 باشد."));
            }

            if (filter?.Length > 100)
            {
                errors.Add(Error.Validation("Filter.TooLong", "حداکثر طول فیلتر 100 کاراکتر است."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateTitleUniquenessAsync(string title)
        {
            var exists = await _imageSiteRepository.ExistsAsync(x => x.Title == title);
            return exists
                ? Error.Validation("ImageSite.TitleDuplicate", "عنوان باید یکتا باشد.")
                : Result.Success;
        }

    }
}
