using ErrorOr;
using Gtm.Contract.UserContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using Utility.Appliation.Security;

namespace Gtm.Application.UserApp.Command
{
    public record EditUserByAdminCommand(EditUserByAdmin UserByAdmin) : IRequest<ErrorOr<Success>>;

    public class EditUserByAdminCommandHandler : IRequestHandler<EditUserByAdminCommand, ErrorOr<Success>>
    {
        private readonly IUserRepo _userRepo;
        private readonly IUserValidator _userValidator;
        private readonly IFileService _fileService;

        public EditUserByAdminCommandHandler(
            IUserRepo userRepo,
            IUserValidator userValidator,
            IFileService fileService)
        {
            _userRepo = userRepo;
            _userValidator = userValidator;
            _fileService = fileService;
        }

        public async Task<ErrorOr<Success>> Handle(EditUserByAdminCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی اولیه
            var validationResult = await _userValidator.ValidateUpdateByAdminAsync(request.UserByAdmin);
            if (validationResult.IsError)
                return validationResult.Errors;

            var user = await _userRepo.GetByIdAsync(request.UserByAdmin.Id);
            if (user == null)
                return Error.NotFound("User.NotFound", "کاربر یافت نشد.");

            // مدیریت تصویر
            string imageName = request.UserByAdmin.AvatarName;
            string oldImageName = request.UserByAdmin.AvatarName;
            bool avatarChanged = false;

            if (request.UserByAdmin.AvatarFile != null)
            {
                imageName = await _fileService.UploadImageAsync(
                    request.UserByAdmin.AvatarFile,
                    FileDirectories.UserImageFolder);

                await _fileService.ResizeImageAsync(imageName, FileDirectories.UserImageFolder, 100);
                avatarChanged = true;
            }

            // مدیریت رمز عبور
            var password = string.IsNullOrEmpty(request.UserByAdmin.Password)
                ? user.Password
                : Sha256Hasher.Hash(request.UserByAdmin.Password);

            // اعمال تغییرات
            user.Edit(
                request.UserByAdmin.FullName,
                request.UserByAdmin.Mobile.Trim(),
                request.UserByAdmin.Email?.ToLower().Trim(),
                password,
                imageName,
                request.UserByAdmin.UserGender
            );

            // ذخیره تغییرات
            var saveResult = await _userRepo.SaveChangesAsync(cancellationToken);
            if (!saveResult)
            {
                // Rollback در صورت خطا
                if (avatarChanged)
                {
                    await RollbackImageUpload(imageName);
                }
                return Error.Failure("User.UpdateFailed", "خطا در ذخیره تغییرات کاربر");
            }

            // حذف تصویر قدیمی در صورت موفقیت
            if (avatarChanged && oldImageName != "default.png")
            {
                await DeleteOldImages(oldImageName);
            }

            return Result.Success;
        }

        private async Task RollbackImageUpload(string imageName)
        {
            await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory, imageName));
            await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory100, imageName));
        }

        private async Task DeleteOldImages(string oldImageName)
        {
            await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory, oldImageName));
            await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory100, oldImageName));
        }
    }
}
