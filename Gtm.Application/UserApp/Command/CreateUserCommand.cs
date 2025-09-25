using ErrorOr;
using Gtm.Contract.UserContract.Command;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Appliation.Security;

namespace Gtm.Application.UserApp.Command
{
    public record CreateUserCommand(CreateUserDto CreateUser) : IRequest<ErrorOr<Success>>;
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ErrorOr<Success>>
    {
        private readonly IUserRepo _userRepo;
        private readonly IFileService _fileService;
        private readonly IUserValidator _validator;

        public CreateUserCommandHandler(IUserRepo userRepository, IFileService fileService, IUserValidator validator)
        {
            _userRepo = userRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateCreateAsync(request.CreateUser);
            if (validationResult.IsError)
                return validationResult.Errors;

            var model = request.CreateUser;

            // آپلود آواتار (در صورت وجود)
            string imageName = "default.png";
            if (model.AvatarFile is not null)
            {
                imageName = await _fileService.UploadImageAsync(model.AvatarFile, FileDirectories.UserImageFolder);
                if (string.IsNullOrWhiteSpace(imageName))
                    return Error.Failure("User.AvatarUploadFailed", "آپلود تصویر با خطا مواجه شد.");

                await _fileService.ResizeImageAsync(imageName, FileDirectories.UserImageFolder, 100);
            }

            // هش کردن رمز عبور
            var hashedPassword = Sha256Hasher.Hash(model.Password);

            // ساخت کاربر
            var code = GenerateRandomCode.GenerateUserRegisterCode().ToString();
            var user = new User(
                model.FullName,
                model.Mobile.Trim(),
                model.Email?.ToLower().Trim(),
                code,
                imageName,
                true,
                false,
                model.UserGender
            );

            user.ChangePassword(hashedPassword);

            await _userRepo.AddAsync(user);
            var created = await _userRepo.SaveChangesAsync(cancellationToken);

            if (!created)
            {
                if (model.AvatarFile is not null)
                {
                    await _fileService.DeleteImageAsync(FileDirectories.UserImageDirectory + imageName);
                    await _fileService.DeleteImageAsync(FileDirectories.UserImageDirectory100 + imageName);
                }

                return Error.Failure("User.CreateFailed", "ثبت کاربر با خطا مواجه شد.");
            }

            return Result.Success;
        }
    }
}