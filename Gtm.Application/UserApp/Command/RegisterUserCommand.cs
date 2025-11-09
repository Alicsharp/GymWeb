using ErrorOr;
using Gtm.Contract.UserContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.Auth;
using Utility.Appliation.Security;
using Utility.Appliation;
using Gtm.Domain.UserDomain.UserDm;

namespace Gtm.Application.UserApp.Command
{
    public record RegisterUserCommand(RegisterUser Command) : IRequest<ErrorOr<bool>>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<bool>>
    {
        private readonly IUserRepo _userRepo;
        private readonly IAuthService _authService;

        public RegisterUserCommandHandler(IUserRepo userRepo, IAuthService authService)
        {
            _userRepo = userRepo;
            _authService = authService;
        }
       

        public async Task<ErrorOr<bool>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var mobile = request.Command.Mobile?.Trim();

            if (string.IsNullOrWhiteSpace(mobile))
                return Error.Validation("User.Mobile.Required", "شماره موبایل الزامی است.");

             
           var key = GenerateRandomCode.GenerateUserRegisterCode().ToString();
           key = "22222";
            var hashedPassword = Sha256Hasher.Hash(key);

            var user = await _userRepo.GetByMobileAsync(mobile);

            if (user is null)
            {
                var newUser = User.Register(mobile, key);
                newUser.ChangePassword(hashedPassword);

                  await _userRepo.AddAsync(newUser);
                var saved= await _userRepo.SaveChangesAsync(cancellationToken);
                if (!saved)
                    return Error.Failure("User.Register.Failed", "ثبت‌نام با خطا مواجه شد.");

                // TODO: ارسال SMS فعال‌سازی
                return true;
            }
            else
            {
                user.ChangePassword(hashedPassword);
                var updated = await _userRepo.SaveChangesAsync(cancellationToken);
                if (!updated)
                    return Error.Failure("User.PasswordReset.Failed", "بازیابی رمز عبور با خطا مواجه شد.");

                // TODO: ارسال SMS فعال‌سازی
                return true;
            }
        }
    }
}
