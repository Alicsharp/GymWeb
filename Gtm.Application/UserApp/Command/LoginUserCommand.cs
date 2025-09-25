using ErrorOr;
using Gtm.Contract.UserContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.Security;
using Utility.Contract;

namespace Gtm.Application.UserApp.Command
{
    public record LoginUserCommand(LoginUser LoginUser) : IRequest<ErrorOr<AuthModel>>;

    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ErrorOr<AuthModel>>
    {
        private readonly IUserRepo _userRepo;


        public LoginUserCommandHandler(IUserRepo userRepository)
        {
            _userRepo = userRepository;

        }

        public async Task<ErrorOr<AuthModel>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var mobile = request.LoginUser.Mobile?.Trim();
            var password = request.LoginUser.Password?.Trim();

            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrWhiteSpace(password))
                return Error.Validation("User.Login.Input.Invalid", "موبایل و رمز عبور الزامی است.");

            var user = await _userRepo.GetByMobileAsync(mobile);

            if (user is null)
                return Error.NotFound("User.NotFound", "کاربری با این شماره یافت نشد.");

            var hashPass = Sha256Hasher.Hash(password);

            if (user.Password != hashPass)
                return Error.Validation("User.Login.PasswordInvalid", "رمز عبور اشتباه است.");

            var authmodel = new AuthModel()
            {
                Avatar = user.Avatar,
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Mobile : user.FullName,
                Mobile = user.Mobile,
                UserId = user.Id
            };

            return authmodel;
        }
    }
}
