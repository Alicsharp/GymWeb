using ErrorOr;
using Gtm.Contract.SiteContract.BanarContract.Command;
using Gtm.Domain.SiteDomain.BannerAgg;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.BannerApp.Command
{
    public record CreateBannerCommand(CreateBaner Command) : IRequest<ErrorOr<Success>>;

    public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, ErrorOr<Success>>
    {
        private readonly IBanerRepository _banerRepository;
        private readonly IFileService _fileService;
        private readonly IBannerValidator _bannerValidator;

        public CreateBannerCommandHandler(IBanerRepository banerRepository,IFileService fileService,IBannerValidator bannerValidator)
        {
            _banerRepository = banerRepository;
            _fileService = fileService;
            _bannerValidator = bannerValidator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _bannerValidator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // آپلود تصویر
            string imageName = await _fileService.UploadImageAsync(request.Command.ImageFile, FileDirectories.BanerImageFolder);
            if (string.IsNullOrEmpty(imageName))
            {
                return Error.Failure("Banner.ImageUploadFailed", "بارگذاری تصویر با خطا مواجه شد.");
            }

            try
            {
                // تغییر اندازه تصویر
                await _fileService.ResizeImageAsync(imageName, FileDirectories.BanerImageFolder, 100);

                // ایجاد بنر
                var banner = new Baner(
                    imageName,
                    request.Command.ImageAlt,
                    request.Command.Url,
                    request.Command.State);

                await _banerRepository.AddAsync(banner);

                // ذخیره تغییرات
                if (!await _banerRepository.SaveChangesAsync(cancellationToken))
                {
                    await RollbackImageOperations(imageName);
                    return Error.Failure("Banner.SaveFailed", "ذخیره بنر با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(imageName);
                return Error.Failure("Banner.OperationFailed", $"خطا در ایجاد بنر: {ex.Message}");
            }
        }

        private async Task RollbackImageOperations(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.BanerImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.BanerImageFolder}{imageName}");
            }
            catch
            {
                // Log the error if needed
            }
        }
    }
}
