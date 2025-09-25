
using Gtm.Application.UserApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.ViewComponents
{
    public class UserPanelSidebarViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        public UserPanelSidebarViewComponent(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetUserPanelSideBarModelQuery(userId: _authService.GetLoginUserId()));
            return View(model.Value);
        }
    }
}
