using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Command;
using Gtm.Domain.SiteDomain.MenuAgg;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.MenuApp.Command
{
    public record CreateMenuCommand(CreateMenu Command) : IRequest<ErrorOr<Success>>;

    public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, ErrorOr<Success>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IFileService _fileService;
        private readonly IMenuValidator _validator;

        public CreateMenuCommandHandler(IMenuRepository menuRepository,IFileService fileService,IMenuValidator validator)
        {
            _menuRepository = menuRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
                return validationResult.Errors;

            string imageName = string.Empty;

            try
            {
                // Upload image if exists
                if (request.Command.ImageFile != null)
                {
                    imageName = await _fileService.UploadImageAsync(request.Command.ImageFile, FileDirectories.MenuImageFolder);
                    if (string.IsNullOrEmpty(imageName))
                        return Error.Failure("Menu.ImageUploadFailed", "بارگذاری تصویر با خطا مواجه شد.");

                    await _fileService.ResizeImageAsync(imageName, FileDirectories.MenuImageFolder, 100);
                }

                // Create menu
                var menu = new Menu(
                    request.Command.Number,
                    request.Command.Title,
                    request.Command.Url,
                    request.Command.Status,
                    imageName,
                    request.Command.ImageAlt,
                    parentId: null);

                // Save to database
                await _menuRepository.AddAsync(menu);
                var saveResult = await _menuRepository.SaveChangesAsync(cancellationToken);

                if (!saveResult)
                {
                    await RollbackImageOperations(imageName, request.Command.ImageFile != null);
                    return Error.Failure("Menu.SaveFailed", "ذخیره منو در پایگاه داده با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(imageName, request.Command.ImageFile != null);
                return Error.Failure("Menu.OperationFailed", $"خطا در ایجاد منو: {ex.Message}");
            }
        }

        private async Task RollbackImageOperations(string imageName, bool imageUploaded)
        {
            if (imageUploaded)
            {
                try
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory}{imageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory100}{imageName}");
                }
                catch { }
              
            }
        }
    }
}
