using ErrorOr;
using Gtm.Contract.SiteContract.BanarContract.Command;
using MediatR;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.BannerApp.Command
{
    public record EditBannerCommand(EditBaner Command) : IRequest<ErrorOr<Success>>;

    public class EditBannerCommandHandler : IRequestHandler<EditBannerCommand, ErrorOr<Success>>
    {
        private readonly IBanerRepository _banerRepository;
        private readonly IFileService _fileService;
        private readonly IBannerValidator _bannerValidator;

        public EditBannerCommandHandler(IBanerRepository banerRepository,IFileService fileService,IBannerValidator bannerValidator)
        {
            _banerRepository = banerRepository;
            _fileService = fileService;
            _bannerValidator = bannerValidator;
        }

        public async Task<ErrorOr<Success>> Handle(EditBannerCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _bannerValidator.ValidateUpdateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var banner = await _banerRepository.GetByIdAsync(request.Command.Id);
            string oldImageName = banner.ImageName;
            string newImageName = oldImageName;

            try
            {
                // آپلود تصویر جدید (در صورت وجود)
                if (request.Command.ImageFile != null)
                {
                    newImageName = await _fileService.UploadImageAsync(request.Command.ImageFile, FileDirectories.BanerImageFolder);
                    if (string.IsNullOrEmpty(newImageName))
                    {
                        return Error.Failure("Banner.ImageUploadFailed", "بارگذاری تصویر با خطا مواجه شد.");
                    }

                    await _fileService.ResizeImageAsync(newImageName, FileDirectories.BanerImageFolder, 100);
                }

                // ویرایش بنر
                banner.Edit(newImageName, request.Command.ImageAlt, request.Command.Url);

                // ذخیره تغییرات
                if (!await _banerRepository.SaveChangesAsync(cancellationToken))
                {
                    await RollbackImageOperations(newImageName, request.Command.ImageFile != null);
                    return Error.Failure("Banner.SaveFailed", "ذخیره بنر با خطا مواجه شد.");
                }

                // حذف تصویر قدیمی (در صورت آپلود تصویر جدید)
                if (request.Command.ImageFile != null)
                {
                    await DeleteOldImages(oldImageName);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(newImageName, request.Command.ImageFile != null);
                return Error.Failure("Banner.OperationFailed", $"خطا در ویرایش بنر: {ex.Message}");
            }
        }

        private async Task DeleteOldImages(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.BanerImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.BanerImageDirectory100}{imageName}");
            }
            catch
            {
                // Log the error if needed
            }
        }

        private async Task RollbackImageOperations(string newImageName, bool isNewImageUploaded)
        {
            try
            {
                if (isNewImageUploaded)
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.BanerImageDirectory}{newImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.BanerImageDirectory100}{newImageName}");
                }
            }
            catch
            {
                // Log the error if needed
            }
        }
    }
}
