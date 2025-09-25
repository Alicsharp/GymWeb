using ErrorOr;

using MediatR;
 
using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using Utility.Appliation.FileService;
using Gtm.Domain.SiteDomain.SiteServiceAgg;
using Utility.Appliation;

namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Command
{
    public record CreateSiteServiceCommand(CreateSiteService Command) : IRequest<ErrorOr<Success>>;

    public class CreateSiteServiceCommandHandler : IRequestHandler<CreateSiteServiceCommand, ErrorOr<Success>>
    {
        private readonly ISiteServiceRepository _siteServiceRepository;
        private readonly IFileService _fileService;
        private readonly ISiteServiceValidator _validator;

        public CreateSiteServiceCommandHandler(ISiteServiceRepository siteServiceRepository,IFileService fileService,ISiteServiceValidator validator)
        {
            _siteServiceRepository = siteServiceRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateSiteServiceCommand request,CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            string imageName = string.Empty;
            try
            {
                // Upload image
                imageName = await _fileService.UploadImageAsync(
                    request.Command.ImageFile,
                    FileDirectories.ServiceImageFolder);

                if (string.IsNullOrEmpty(imageName))
                {
                    return Error.Failure(
                        "SiteService.UploadFailed",
                        "آپلود تصویر با خطا مواجه شد.");
                }

                // Resize image
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ServiceImageFolder, 100);

                // Create service
                var service = new SiteService(
                    imageName,
                    request.Command.ImageAlt,
                    request.Command.Title.Trim());

                // Save to database
                await _siteServiceRepository.AddAsync(service);
                var saveResult = await _siteServiceRepository.SaveChangesAsync(cancellationToken);

                if (!saveResult)
                {
                    await RollbackImageOperations(imageName);
                    return Error.Failure(
                        "SiteService.SaveFailed",
                        "ذخیره سرویس با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(imageName);
                return Error.Failure(
                    "SiteService.OperationFailed",
                    $"خطا در ایجاد سرویس: {ex.Message}");
            }
        }

        private async Task RollbackImageOperations(string imageName)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageName))
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.ServiceImageDirectory}{imageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ServiceImageDirectory100}{imageName}");
                }
            }
            catch
            {
                // Log error if needed
            }
        }
    }
}
