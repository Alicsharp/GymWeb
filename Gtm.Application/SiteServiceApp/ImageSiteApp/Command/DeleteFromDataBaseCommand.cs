using ErrorOr;
using MediatR;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.ImageSiteApp.Command
{
    public record DeleteFromDataBaseCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class DeleteFromDataBaseCommandHandler : IRequestHandler<DeleteFromDataBaseCommand, ErrorOr<Success>>
    {
        private readonly IImageSiteRepository _imageSiteRepository;
        private readonly IFileService _fileService;
        private readonly IImageSiteValidator _validator;

        public DeleteFromDataBaseCommandHandler(IImageSiteRepository imageSiteRepository,IFileService fileService,IImageSiteValidator validator)
        {
            _imageSiteRepository = imageSiteRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteFromDataBaseCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateDeleteAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // دریافت تصویر
                var image = await _imageSiteRepository.GetByIdAsync(request.Id);
                string imageName = image.ImageName;

                // حذف از دیتابیس
                _imageSiteRepository.Remove(image);
                var isDeleted = await _imageSiteRepository.SaveChangesAsync(cancellationToken);

                if (!isDeleted)
                {
                    return Error.Failure("ImageSite.DeleteFailed", "حذف تصویر از پایگاه داده با خطا مواجه شد.");
                }

                // حذف فایل‌ها
                await DeleteImageFiles(imageName);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure("ImageSite.DeleteError", $"خطا در حذف تصویر: {ex.Message}");
            }
        }

        private async Task DeleteImageFiles(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ImageDirectory100}{imageName}");
            }
            catch (Exception ex)
            {
                // Log the error if needed
                Console.WriteLine($"Error deleting image files: {ex.Message}");
            }
        }
    }
}
