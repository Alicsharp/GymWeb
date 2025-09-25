using ErrorOr;
using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Command
{
    public record EditSiteServiceCommand(EditSiteService Command) : IRequest<ErrorOr<Success>>;

    public class EditSiteServiceCommandHandler : IRequestHandler<EditSiteServiceCommand, ErrorOr<Success>>
    {
        private readonly ISiteServiceRepository _siteServiceRepository;
        private readonly IFileService _fileService;
        private readonly ISiteServiceValidator _validator;

        public EditSiteServiceCommandHandler(ISiteServiceRepository siteServiceRepository,IFileService fileService,ISiteServiceValidator validator)
        {
            _siteServiceRepository = siteServiceRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(
            EditSiteServiceCommand request,
            CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateEditAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // Get existing service
            var service = await _siteServiceRepository.GetByIdAsync(request.Command.Id);
            if (service == null)
            {
                return Error.NotFound(
                    "SiteService.NotFound",
                    "سرویس با این شناسه یافت نشد.");
            }

            string oldImageName = service.ImageName;
            string newImageName = oldImageName;
            bool imageUploaded = false;

            try
            {
                // Handle image upload if new image provided
                if (request.Command.ImageFile != null)
                {
                    newImageName = await _fileService.UploadImageAsync(
                        request.Command.ImageFile,
                        FileDirectories.ServiceImageFolder);

                    if (string.IsNullOrEmpty(newImageName))
                    {
                        return Error.Failure(
                            "SiteService.UploadFailed",
                            "آپلود تصویر جدید با خطا مواجه شد.");
                    }

                    await _fileService.ResizeImageAsync(newImageName, FileDirectories.ServiceImageFolder, 100);
                    imageUploaded = true;
                }

                // Update service properties
                service.Edit(
                    newImageName,
                    request.Command.ImageAlt,
                    request.Command.Title.Trim());

                // Save changes
                var result = await _siteServiceRepository.SaveChangesAsync(cancellationToken);
                if (!result)
                {
                    await RollbackImageOperations(newImageName, imageUploaded);
                    return Error.Failure(
                        "SiteService.SaveFailed",
                        "ذخیره تغییرات سرویس با خطا مواجه شد.");
                }

                // Delete old image if new image was uploaded
                if (imageUploaded && !string.IsNullOrEmpty(oldImageName))
                {
                    await DeleteOldImages(oldImageName);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(newImageName, imageUploaded);
                return Error.Failure(
                    "SiteService.OperationFailed",
                    $"خطا در ویرایش سرویس: {ex.Message}");
            }
        }

        private async Task DeleteOldImages(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ServiceImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ServiceImageDirectory100}{imageName}");
            }
            catch
            {
                // Log error if needed
            }
        }

        private async Task RollbackImageOperations(string imageName, bool isNewImage)
        {
            if (isNewImage && !string.IsNullOrEmpty(imageName))
            {
                try
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.ServiceImageDirectory}{imageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ServiceImageDirectory100}{imageName}");
                }
                catch
                {
                    // Log error if needed
                }
            }
        }
    }
}
