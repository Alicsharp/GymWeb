using ErrorOr;
using Gtm.Contract.PostContract.UserPostContract.Command;
using Gtm.Domain.PostDomain.UserPostAgg;
using MediatR;
using Utility.Appliation.FileService;

namespace Gtm.Application.PostServiceApp.PackageApp.Command
{
  public record CreatePackgeCommand(CreatePackage createPackage) : IRequest<ErrorOr<Success>>;
    public class CreatePackgeCommandHandler : IRequestHandler<CreatePackgeCommand, ErrorOr<Success>>
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IFileService _fileService;

        public CreatePackgeCommandHandler(IPackageRepo packageRepository,IFileService fileService)
        {
            _packageRepository = packageRepository;
            _fileService = fileService;
        }

        public async Task<ErrorOr<Success>> Handle(CreatePackgeCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی عنوان تکراری
                if (await _packageRepository.ExistsAsync(p =>
                    p.Title.Trim() == request.createPackage.Title.Trim()))
                {
                    return Error.Conflict(
                        code: "Package.DuplicateTitle",
                        description: "عنوان پکیج نمی‌تواند تکراری باشد");
                }

                // آپلود تصویر
                string imageName = await _fileService.UploadImageAsync(request.createPackage.ImageFile,FileDirectories.PackageImageFolder);

                if (string.IsNullOrEmpty(imageName))
                {
                    return Error.Validation(
                        code: "Package.ImageRequired",
                        description: "تصویر پکیج الزامی است");
                }

                // ایجاد نسخه‌های مختلف از تصویر
                await _fileService.ResizeImageAsync(imageName, FileDirectories.PackageImageFolder, 400);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.PackageImageFolder, 100);

                // ایجاد پکیج جدید
                var package = new Package(
                    request.createPackage.Title,
                    request.createPackage.Description,
                    request.createPackage.Count,
                    request.createPackage.Price,
                    imageName,
                    request.createPackage.ImageAlt);

                // ذخیره پکیج
                await _packageRepository.AddAsync(package);
                var createResult=await _packageRepository.SaveChangesAsync(cancellationToken);

                if (!createResult)
                {
                    // پاک کردن تصاویر در صورت شکست
                    await CleanupImages(imageName);

                    return Error.Failure(
                        code: "Package.CreationFailed",
                        description: "خطا در ایجاد پکیج");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Package.UnexpectedError",
                    description: $"خطای غیرمنتظره در ایجاد پکیج: {ex.Message}");
            }
        }

        private async Task CleanupImages(string imageName)
        {
            try
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory400}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory100}{imageName}");
            }
            catch
            {
                // Log cleanup errors if needed
            }
        }
    }

}
