using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Command;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.MenuApp.Command
{
    public record EditMenuCommand(EditMenu command) : IRequest<ErrorOr<Success>>;
    public class EditMenuCommandHandler : IRequestHandler<EditMenuCommand, ErrorOr<Success>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IFileService _fileService;
        private readonly IMenuValidator _validator;

        public EditMenuCommandHandler(IMenuRepository menuRepository,IFileService fileService,IMenuValidator validator)
        {
            _menuRepository = menuRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(EditMenuCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateEditAsync(request.command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var menu = await _menuRepository.GetByIdAsync(request.command.Id);
            string oldImageName = menu.ImageName;
            string newImageName = oldImageName;

            try
            {
                // Handle image upload if new image provided
                if (request.command.ImageFile != null)
                {
                    newImageName = await _fileService.UploadImageAsync(
                        request.command.ImageFile,
                        FileDirectories.MenuImageFolder);

                    if (string.IsNullOrEmpty(newImageName))
                    {
                        return Error.Failure("Menu.ImageUploadFailed", "بارگذاری تصویر با خطا مواجه شد.");
                    }

                    await _fileService.ResizeImageAsync(newImageName, FileDirectories.MenuImageDirectory100, 100);
                }

                // Update menu properties
                menu.Edit(request.command.Number,request.command.Title,request.command.Url,newImageName,request.command.ImageAlt);

                // Save changes
                var saveResult = await _menuRepository.SaveChangesAsync(cancellationToken);
                if (!saveResult)
                {
                    await RollbackImageOperations(newImageName, request.command.ImageFile != null);
                    return Error.Failure("Menu.SaveFailed", "ذخیره تغییرات منو با خطا مواجه شد.");
                }

                // Delete old image if new image was uploaded
                if (request.command.ImageFile != null && !string.IsNullOrEmpty(oldImageName))
                {
                    await DeleteOldImages(oldImageName);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(newImageName, request.command.ImageFile != null);
                return Error.Failure("Menu.UpdateFailed", $"خطا در به‌روزرسانی منو: {ex.Message}");
            }
        }

        private async Task DeleteOldImages(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory100}{imageName}");
            }
            catch
            {
                 
            }
        }

        private async Task RollbackImageOperations(string imageName, bool isNewImage)
        {
            if (isNewImage && !string.IsNullOrEmpty(imageName))
            {
                try
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory}{imageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory100}{imageName}");
                }
                catch
                {
                    
                }
            }
        }
    }
}
