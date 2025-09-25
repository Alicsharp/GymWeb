using ErrorOr;
using Gtm.Contract.SiteContract.ImageSiteContract.Command;
using Gtm.Domain.SiteDomain.SiteImageAgg;
using MediatR;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.ImageSiteApp.Command
{
    public record CreateImageSiteCommand(CreateImageSite Command) : IRequest<ErrorOr<Success>>;

    public class CreateImageSiteCommandHandler : IRequestHandler<CreateImageSiteCommand, ErrorOr<Success>>
    {
        private readonly IImageSiteRepository _imageSiteRepository;
        private readonly IFileService _fileService;
        private readonly IImageSiteValidator _validator;

        public CreateImageSiteCommandHandler(IImageSiteRepository imageSiteRepository,IFileService fileService,IImageSiteValidator validator)
        {
            _imageSiteRepository = imageSiteRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateImageSiteCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // آپلود تصویر
            string imageName = await _fileService.UploadImageAsync(request.Command.ImageFile, FileDirectories.ImageFolder);
            if (string.IsNullOrEmpty(imageName))
            {
                return Error.Failure("ImageSite.UploadFailed", "بارگذاری تصویر با خطا مواجه شد.");
            }

            try
            {
                // تغییر اندازه تصویر
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ImageFolder, 100);

                // ایجاد و ذخیره تصویر
                var image = new SiteImage(imageName, request.Command.Title);
                 await _imageSiteRepository.AddAsync(image);
                var isCreated = await _imageSiteRepository.SaveChangesAsync(cancellationToken);
                if (!isCreated)
                {
                    await RollbackImageOperations(imageName);
                    return Error.Failure("ImageSite.SaveFailed", "ذخیره تصویر در پایگاه داده با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(imageName);
                return Error.Failure("ImageSite.OperationFailed", $"خطا در عملیات: {ex.Message}");
            }
        }

        private async Task RollbackImageOperations(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ImageDirectory100}{imageName}");
            }
            catch
            {
                // Log error if needed
            }
        }
    }
}
