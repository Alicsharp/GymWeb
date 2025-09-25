using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Contract.UserContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserApp.Query
{
    public record GetForEditByUserQuery(int userId) : IRequest<ErrorOr<EditUserByUser>>;
    public class GetForEditByUserQueryHandler : IRequestHandler<GetForEditByUserQuery, ErrorOr<EditUserByUser>>
    {
        private readonly IUserRepo _userRepository;

        public GetForEditByUserQueryHandler(IUserRepo userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ErrorOr<EditUserByUser>> Handle(GetForEditByUserQuery request, CancellationToken cancellationToken)
        {
            if (request.userId == null)
            {
                return Error.Failure("NotFound", "we con`t find a user");
            }
            var User = await _userRepository.GetByIdAsync(request.userId);
            return new EditUserByUser()
            {
                Id = User.Id,
                AvatarFile = null,

                AvatarName = User.Avatar,
                Email = User.Email,
                FullName = User.FullName,
                Mobile = User.Mobile,
                UserGender = User.UserGender
            };
        }
    }
}
