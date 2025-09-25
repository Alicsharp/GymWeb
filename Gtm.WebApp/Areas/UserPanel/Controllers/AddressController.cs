using Gtm.Application.PostServiceApp.CityApp.Command;
using Gtm.Application.PostServiceApp.StateApp.Command;
using Gtm.Application.UserAddressApp.Command;
using Gtm.Application.UserAddressApp.Query;
using Gtm.Contract.UserAddressContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
    [Authorize]
    [Area("UserPanel")]
    public class AddressController : Controller
    {
        private int _userId;
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;

        public AddressController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }


        public async Task<IActionResult> Index()
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetAddresseForUserPanelQuery(_userId));
            return View(model.Value);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateAddress model)
        {
            _userId = _authService.GetLoginUserId();
            model.UserId = _userId;
            if (!ModelState.IsValid) return View(model);
            var s = await _mediator.Send(new IsStateCorrectCommand(model.StateId));
            if (s.IsError==true)
            {
                ModelState.AddModelError(nameof(model.StateId), "لطفا یک استان انتخاب کنید");
                return View(model);
            }
            var b = await _mediator.Send(new IsCityCorrectCommand(model.StateId, model.CityId));
            if (b.IsError == true)
            {
                ModelState.AddModelError(nameof(model.CityId), "لطفا یک شهر انتخاب کنید");
                return View(model);
            }

            var res = await _mediator.Send(new CreateUserAddressCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = "true";
                return RedirectToAction("Index", "Address");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<bool> Delete(int id)
        {
            _userId = _authService.GetLoginUserId();
            var result = await _mediator.Send(new DeleteUserAddressCommand(id, _userId));
            return result.Value;
        }
    }
}
