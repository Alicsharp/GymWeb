using Gtm.Application.UserApp.Command;
using Gtm.Application.UserApp.Query;
using Gtm.Contract.UserContract.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
    [Authorize]

    [Area("UserPanel")]
    public class HomeController : Controller
    {
        private int _userId;

        private readonly IAuthService _authService;
        private readonly IMediator _mediator;

        public HomeController(IAuthService authService, IMediator mediator)
        {
            _authService = authService;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetUserInfoForPanelQuery(_userId));
            return View(model.Value);
        }
        public async Task<IActionResult> EditProfile()
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetForEditByUserQuery(_userId));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditUserByUser model)
        {
            _userId = _authService.GetLoginUserId();
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditByUserCommand(model));
            if (res.IsError == false)
            {
                _authService.Logout();
                TempData["SuccessEditProfile"] = true;
                return Redirect("/Auth/Login");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
    }
}
