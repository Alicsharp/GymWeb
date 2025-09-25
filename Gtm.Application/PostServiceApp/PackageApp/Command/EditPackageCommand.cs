using ErrorOr;
using Gtm.Contract.PostContract.UserPostContract.Command;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.PostServiceApp.PackageApp.Command
{
 
    public record EditPackageCommand(EditPackage Command) : IRequest<ErrorOr<Success>>;

    public class EditPackageCommandHandler : IRequestHandler<EditPackageCommand, ErrorOr<Success>>
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IFileService _fileService;
        private readonly IPackageValidation _packageValidation;

        public EditPackageCommandHandler(IPackageRepo packageRepository,IFileService fileService,IPackageValidation packageValidation)
        {
            _packageRepository = packageRepository;
            _fileService = fileService;
            _packageValidation = packageValidation;
        }

        public async Task<ErrorOr<Success>> Handle(
            EditPackageCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _packageValidation.ValidateEditPackage(
                    request.Command,
                    request.Command.Id);

                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                var package = await _packageRepository.GetByIdAsync(request.Command.Id);
                string oldImageName = package.ImageName;
                string newImageName = oldImageName;

                // مدیریت تصویر
                if (request.Command.ImageFile != null)
                {
                    newImageName = await HandleImageUpload(request.Command.ImageFile);
                    if (string.IsNullOrEmpty(newImageName))
                    {
                        return Error.Validation(
                            code: "Package.ImageUploadFailed",
                            description: "خطا در آپلود تصویر");
                    }
                }

                // اعمال تغییرات
                package.Edit(
                    request.Command.Title,
                    request.Command.Description,
                    request.Command.Count,
                    request.Command.Price,
                    newImageName,
                    request.Command.ImageAlt);

                // ذخیره تغییرات
                if (!await _packageRepository.SaveChangesAsync(cancellationToken))
                {
                    await CleanupNewImageIfUploaded(request.Command.ImageFile, newImageName);
                    return Error.Failure(
                        code: "Package.UpdateFailed",
                        description: "خطا در بروزرسانی پکیج");
                }

                // پاک کردن تصویر قدیمی اگر تصویر جدید آپلود شده بود
                if (request.Command.ImageFile != null)
                {
                    await CleanupOldImage(oldImageName);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Package.UnexpectedError",
                    description: $"خطای غیرمنتظره در ویرایش پکیج: {ex.Message}");
            }
        }

        private async Task<string> HandleImageUpload(IFormFile imageFile)
        {
            var imageName = await _fileService.UploadImageAsync(
                imageFile,
                FileDirectories.PackageImageFolder);

            if (!string.IsNullOrEmpty(imageName))
            {
                await Task.WhenAll(
                    _fileService.ResizeImageAsync(imageName, FileDirectories.PackageImageFolder, 400),
                    _fileService.ResizeImageAsync(imageName, FileDirectories.PackageImageFolder, 100));
            }

            return imageName;
        }

        private async Task CleanupOldImage(string imageName)
        {
            await Task.WhenAll(
                _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory}{imageName}"),
                _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory400}{imageName}"),
                _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory100}{imageName}"));
        }

        private async Task CleanupNewImageIfUploaded(IFormFile imageFile, string imageName)
        {
            if (imageFile != null && !string.IsNullOrEmpty(imageName))
            {
                await Task.WhenAll(
                    _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory}{imageName}"),
                    _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory400}{imageName}"),
                    _fileService.DeleteImageAsync($"{FileDirectories.PackageImageDirectory100}{imageName}"));
            }
        }
    }
}
