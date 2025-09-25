using ErrorOr;
using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Application.RoleApp;
using Gtm.Application.SeoApp;
using Gtm.Contract.UiContract;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserApp.Query
{
    public record GetUserPanelSideBarModelQuery(int userId) : IRequest<ErrorOr<UserPanelSideBarQueryModel>>;
    public class GetUserPanelSideBarModelQueryHanler : IRequestHandler<GetUserPanelSideBarModelQuery, ErrorOr<UserPanelSideBarQueryModel>>
    {
        private readonly IPostOrderRepo _postOrderRepository;
        private readonly IUserRepo _userRepository;
        private readonly IRoleRepo _roleRepository;
        

        public GetUserPanelSideBarModelQueryHanler(IPostOrderRepo postOrderRepository, IUserRepo userRepository, IRoleRepo roleRepository )
        {
            _postOrderRepository = postOrderRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            
        }

        public async Task<ErrorOr<UserPanelSideBarQueryModel>> Handle(GetUserPanelSideBarModelQuery request, CancellationToken cancellationToken)
        {
            bool haveOrderPost = await _postOrderRepository.ExistsAsync(o => o.UserId == request.userId);
            bool isAdmin = await _roleRepository.IsUserAdmin(request.userId);
            //bool havesaller = await _sellerRepository.ExistByAsync(s => s.UserId == request.userId);
            return new UserPanelSideBarQueryModel()
            {
                HaveUserOrderPost = haveOrderPost,
                IsUserAdmin = true,
                HaveUserSeller = false
            }; ;
        }
    }
}
