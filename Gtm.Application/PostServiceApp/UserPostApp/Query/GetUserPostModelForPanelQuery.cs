using ErrorOr;
using Gtm.Application.PostServiceApp.PostSettingApp;
using Gtm.Contract.PostContract.UserPostContract.Query;
using Gtm.Domain.PostDomain.UserPostAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.UserPostApp.Query
{
    public record GetUserPostModelForPanelQuery(int userId) : IRequest<ErrorOr<UserPostPanelModel>>;
    public class GetUserPostModelForPanelQueryHandler : IRequestHandler<GetUserPostModelForPanelQuery, ErrorOr<UserPostPanelModel>>
    {
        private readonly IUserPostRepo _userPostRepository;
        private readonly IPostSettingRepo _postSettingRepository;

        public GetUserPostModelForPanelQueryHandler(IUserPostRepo userPostRepository, IPostSettingRepo postSettingRepository)
        {
            _userPostRepository = userPostRepository;
            _postSettingRepository = postSettingRepository;
        }

        public async Task<ErrorOr<UserPostPanelModel>> Handle(GetUserPostModelForPanelQuery request, CancellationToken cancellationToken)
        {
            UserPost userPost = await _userPostRepository.GetForUserAsync(request.userId);
            var setting = await _postSettingRepository.GetSingleAsync();
            UserPostPanelModel model = new UserPostPanelModel(setting.ApiDescription, userPost.Count, userPost.ApiCode);
            return model;
        }
    }
}
