using ErrorOr;
using Gtm.Contract.UserContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserApp.Query
{
    public record GetUserForEditByAdminQuery(int userId) : IRequest<ErrorOr<EditUserByAdmin>>;
    public class GetUserForEditByAdminQueryHandler : IRequestHandler<GetUserForEditByAdminQuery, ErrorOr<EditUserByAdmin>>
    {
        private readonly IUserRepo _userRepo;
        private readonly IUserValidator _userValidator;

        public GetUserForEditByAdminQueryHandler(IUserRepo userRepo, IUserValidator userValidator)
        {
            _userRepo = userRepo;
            _userValidator = userValidator;
        }

        public async Task<ErrorOr<EditUserByAdmin>> Handle(GetUserForEditByAdminQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _userValidator.ValidateIdAsync(request.userId);
            if(validationResult.IsError)
            {
                return validationResult.Errors;
            }
            var entity= await _userRepo.GetByIdAsync(request.userId);   
            if(entity == null)
            {
       
            return Error.Failure("UserNotFound","کاربر مورد نظر پیدا نشد");

            }
            return new EditUserByAdmin()
            {
                Id = entity.Id,
                AvatarFile = null,
                AvatarName = entity.Avatar,
                Email = entity.Email,
                FullName = entity.FullName,
                Mobile = entity.Mobile,
                Password=null,
                UserGender = entity.UserGender
            };

        }
    }
}
