using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Command;
using Gtm.Domain.SiteDomain.SiteSettingAgg;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Command
{
        public record CreateUbsertCommand(UbsertSiteSetting Command) : IRequest<ErrorOr<Success>>;

        public class CreateUbsertCommandHandler : IRequestHandler<CreateUbsertCommand, ErrorOr<Success>>
        {
            private readonly ISiteSettingRepository _siteSettingRepository;
            private readonly IFileService _fileService;
            private readonly ISiteSettingValidator _validator;

            public CreateUbsertCommandHandler(ISiteSettingRepository siteSettingRepository,IFileService fileService,ISiteSettingValidator validator)
            {
                _siteSettingRepository = siteSettingRepository;
                _fileService = fileService;
                _validator = validator;
            }

            public async Task<ErrorOr<Success>> Handle(CreateUbsertCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateUpsertAsync(request.Command);
                if (validationResult.IsError)
                    return validationResult.Errors;

                try
                {
                    var site = await _siteSettingRepository.GetSingleAsync();
                    if (site == null)
                        return Error.NotFound("SiteSetting.NotFound", "تنظیمات سایت یافت نشد");

                    var oldLogoName = site.LogoName;
                    var oldFavIconName = site.FavIcon;
                    string newLogoName = null;
                    string newFavIconName = null;

                    // آپلود لوگوی جدید
                    if (request.Command.LogoFile != null)
                    {
                        newLogoName = await _fileService.UploadImageAsync(request.Command.LogoFile, FileDirectories.SiteImageFolder);
                        if (string.IsNullOrEmpty(newLogoName))
                            return Error.Failure("File.UploadFailed", "آپلود لوگو ناموفق بود");

                        await _fileService.ResizeImageAsync(newLogoName, FileDirectories.SiteImageFolder, 300);
                    }

                    // آپلود فاوآیکون جدید
                    if (request.Command.FavIconFile != null)
                    {
                        newFavIconName = await _fileService.UploadImageAsync(request.Command.FavIconFile, FileDirectories.SiteImageFolder);
                        if (string.IsNullOrEmpty(newFavIconName))
                        {
                            if (newLogoName != null)
                                await RollbackFile(newLogoName, FileDirectories.SiteImageFolder);
                            return Error.Failure("File.UploadFailed", "آپلود فاوآیکون ناموفق بود");
                        }

                        await _fileService.ResizeImageAsync(newFavIconName, FileDirectories.SiteImageFolder, 64);
                        await _fileService.ResizeImageAsync(newFavIconName, FileDirectories.SiteImageFolder, 32);
                        await _fileService.ResizeImageAsync(newFavIconName, FileDirectories.SiteImageFolder, 16);
                    }

                    // اعمال تغییرات
                    site.Edit(
                        request.Command.Instagram,
                        request.Command.WhatsApp,
                        request.Command.Telegram,
                        request.Command.Youtube,
                        newLogoName ?? oldLogoName,
                        request.Command.LogoAlt,
                        newFavIconName ?? oldFavIconName,
                        request.Command.Enamad,
                        request.Command.SamanDehi,
                        request.Command.SeoBox,
                        request.Command.Android,
                        request.Command.IOS,
                        request.Command.FooterDescription,
                        request.Command.FooterTitle,
                        request.Command.Phone1,
                        request.Command.Phone2,
                        request.Command.Email1,
                        request.Command.Email2,
                        request.Command.Address,
                        request.Command.ContactDescription,
                        request.Command.AboutDescription,
                        request.Command.AboutTitle);

                    if (!await _siteSettingRepository.SaveChangesAsync(cancellationToken))
                    {
                        if (newLogoName != null)
                            await RollbackFile(newLogoName, FileDirectories.SiteImageFolder);
                        if (newFavIconName != null)
                            await RollbackFile(newFavIconName, FileDirectories.SiteImageFolder);
                        return Error.Failure("SiteSetting.SaveFailed", "ذخیره تنظیمات در پایگاه داده ناموفق بود");
                    }

                    // حذف فایل‌های قدیمی پس از موفقیت آمیز بودن عملیات
                    if (newLogoName != null && !string.IsNullOrEmpty(oldLogoName))
                        await _fileService.DeleteImageAsync($"Images/{FileDirectories.SiteImageFolder}/{oldLogoName}");

                    if (newFavIconName != null && !string.IsNullOrEmpty(oldFavIconName))
                        await _fileService.DeleteImageAsync($"Images/{FileDirectories.SiteImageFolder}/{oldFavIconName}");

                    return Result.Success;
                }
                catch (Exception ex)
                {
                    return Error.Failure("SiteSetting.UnexpectedError", $"خطای غیرمنتظره: {ex.Message}");
                }
            }

            private async Task RollbackFile(string fileName, string folder)
            {
                await _fileService.DeleteImageAsync($"Images/{folder}/{fileName}");
                await _fileService.DeleteImageAsync($"Images/{folder}/300/{fileName}");
                await _fileService.DeleteImageAsync($"Images/{folder}/64/{fileName}");
                await _fileService.DeleteImageAsync($"Images/{folder}/32/{fileName}");
                await _fileService.DeleteImageAsync($"Images/{folder}/16/{fileName}");
            }
        }
    }
 
