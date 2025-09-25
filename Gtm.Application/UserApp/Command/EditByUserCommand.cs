using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Contract.UserContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.UserApp.Command
{
    public record EditByUserCommand(EditUserByUser EditUserByUser) : IRequest<ErrorOr<Success>>;
    public class EditByUserCommandHandler : IRequestHandler<EditByUserCommand, ErrorOr<Success>>
    {
        private readonly IUserRepo _userRepository;
        private readonly IFileService _fileService;
        private readonly IUserValidator _userValidator;

        public EditByUserCommandHandler(IUserRepo userRepository,IFileService fileService,IUserValidator userValidator)
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _userValidator = userValidator;
        }

        public async Task<ErrorOr<Success>> Handle(EditByUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _userValidator.ValidateEditByUserAsync(request.EditUserByUser);
            if (validationResult.IsError)
                return validationResult.Errors;

            var model = request.EditUserByUser;
            var user = await _userRepository.GetByIdAsync(model.Id);

            // مدیریت تصویر
            string imageName = model.AvatarName;
            string oldImageName = model.AvatarName;
            bool avatarChanged = false;

            if (model.AvatarFile != null)
            {
                if (!model.AvatarFile.IsImage())
                    return Error.Validation("User.Avatar.Invalid", "تصویر آپلودی معتبر نیست.");

                if (model.AvatarFile.Length > 2 * 1024 * 1024) // 2MB
                    return Error.Validation("User.Avatar.TooLarge", "حجم تصویر نباید بیشتر از 2 مگابایت باشد.");

                imageName = await _fileService.UploadImageAsync(model.AvatarFile, FileDirectories.UserImageFolder);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.UserImageFolder, 100);
                avatarChanged = true;
            }

            // اعمال تغییرات
            user.Edit(model.FullName, model.Mobile, model.Email, user.Password, imageName, model.UserGender);

            var saved = await _userRepository.SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                // Rollback تصویر جدید در صورت خطا
                if (avatarChanged)
                {
                    await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory, imageName));
                    await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory100, imageName));
                }
                return Error.Failure("User.UpdateFailed", "ویرایش اطلاعات با خطا مواجه شد.");
            }

            // حذف تصویر قبلی در صورت موفقیت
            if (avatarChanged && oldImageName != "default.png")
            {
                await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory, oldImageName));
                await _fileService.DeleteImageAsync(Path.Combine(FileDirectories.UserImageDirectory100, oldImageName));
            }

            return Result.Success;
        }
    }
}
