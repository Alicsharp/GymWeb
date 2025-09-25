using ErrorOr;
using Gtm.Contract.UserContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.UserApp.Query
{
    public record GetUserInfoForPanelQuery(int userId) : IRequest<ErrorOr<UserInfoForPanelQueryModel>>;
    public class GetUserInfoForPanelQueryHandler : IRequestHandler<GetUserInfoForPanelQuery, ErrorOr<UserInfoForPanelQueryModel>>
    {
        private readonly IUserRepo _userRepository;
        private readonly IUserValidator _userValidator;

        public GetUserInfoForPanelQueryHandler(IUserRepo userRepository, IUserValidator userValidator)
        {
            _userRepository = userRepository;
            _userValidator = userValidator;
        }

        public async Task<ErrorOr<UserInfoForPanelQueryModel>> Handle(GetUserInfoForPanelQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _userValidator.ValidateGetUserInfoForPanelAsync(request.userId);
            if (validationResult.IsError)
                return validationResult.Errors;

            var user = await _userRepository.GetByIdAsync(request.userId);

            return new UserInfoForPanelQueryModel(
                user.FullName,
                user.Mobile,
                user.Email,
                user.UserGender,
                0,
                0,
                user.CreateDate.ToPersainDate());
        }

       
    }
}
