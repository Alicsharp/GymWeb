using ErrorOr;
 
using Gtm.Application.SiteServiceApp.MenuApp;
using Gtm.Contract.SiteContract.MenuContract.Command;
using Gtm.Domain.SiteDomain.MenuAgg;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;
 
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.MenuApp.Command
{
    public record CreateSubCommand(CreateSubMenu command) : IRequest<ErrorOr<Success>>;

    public class CreateSubCommandHandler : IRequestHandler<CreateSubCommand, ErrorOr<Success>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IFileService _fileService;

        public CreateSubCommandHandler(IMenuRepository menuRepository, IFileService fileService)
        {
            _menuRepository = menuRepository;
            _fileService = fileService;
        }

        public async Task<ErrorOr<Success>> Handle(CreateSubCommand request, CancellationToken cancellationToken)
        {
            var command = request.command;

            // اعتبارسنجی تصویر
            if (command.ParentStatus == MenuStatus.منوی_وبلاگ_با_زیر_منوی_عکس_دار)
            {
                if (command.ImageFile == null || !command.ImageFile.IsImage())
                    return Error.Validation("Image.Invalid", "تصویر معتبر نیست.");

                if (string.IsNullOrWhiteSpace(command.ImageAlt))
                    return Error.Validation("Image.AltMissing", "متن جایگزین تصویر الزامی است.");
            }
            else
            {
                if (command.ImageFile != null)
                    return Error.Validation("Image.NotAllowed", "ارسال تصویر مجاز نیست.");

                if (!string.IsNullOrWhiteSpace(command.ImageAlt))
                    return Error.Validation("Image.AltNotAllowed", "متن جایگزین نباید ارسال شود.");
            }

            // بارگذاری و تغییر اندازه تصویر
            string imageName = "";
            if (command.ImageFile != null)
            {
                imageName = await _fileService.UploadImageAsync(command.ImageFile, FileDirectories.MenuImageFolder);
                if (string.IsNullOrEmpty(imageName))
                    return Error.Failure("Image.UploadFailed", "بارگذاری تصویر با خطا مواجه شد.");

                await _fileService.ResizeImageAsync(imageName, FileDirectories.MenuImageFolder, 100);
            }

            // تعیین وضعیت منو
            MenuStatus status = command.ParentStatus switch
            {
                MenuStatus.منوی_اصلی_با_زیر_منو => MenuStatus.زیرمنوی_سردسته,
                MenuStatus.زیرمنوی_سردسته => MenuStatus.زیرمنو,
                MenuStatus.تیتر_منوی_فوتر => MenuStatus.منوی_فوتر,
                MenuStatus.منوی_وبلاگ_با_زیرمنوی_بدون_عکس => MenuStatus.زیر_منوی_وبلاگ,
                MenuStatus.منوی_وبلاگ_با_زیر_منوی_عکس_دار => MenuStatus.زیر_منوی_وبلاگ,
                _ => throw new InvalidOperationException("وضعیت منو نامعتبر است.")
            };

            var menu = new Menu(command.Number, command.Title, command.Url, status, imageName, command.ImageAlt, command.ParentId);
            
            await _menuRepository.AddAsync(menu);
            var saveResult = await _menuRepository.SaveChangesAsync(cancellationToken);

            if (saveResult)
                return Result.Success;
           
            // حذف تصویر در صورت شکست ذخیره
            if (command.ImageFile != null)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.MenuImageDirectory100}{imageName}");
            }

            return Error.Failure("Menu.SaveFailed", "ذخیره منو با شکست مواجه شد.");
        }
    }

}
