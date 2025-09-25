using Gtm.Application.PostServiceApp.CityApp.Command;
using Gtm.Application.ShopApp.SellerApp.Command;
using Gtm.Application.ShopApp.SellerApp.Query;
using Gtm.Contract.SellerContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers.Seller
{
    [Area("UserPanel")]
    [Authorize]
    public class SellerController : Controller
    {
      
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;
        private int _userId;

        public SellerController(IAuthService authService, IMediator mediator)
        {
            _authService = authService;
            _mediator = mediator;
        }

   

        public async Task<IActionResult> Index()
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetSellersForUserQuery(_userId));
            return View(model.Value);
        }
        public IActionResult Request() => View();
        [HttpPost]
        public async Task<IActionResult> Request(RequestSeller model)
        {
            _userId = _authService.GetLoginUserId();

            if (!ModelState.IsValid)
                return View(model);

            var res = await _mediator.Send(new RequestSellerCommand(_userId, model));

            if (!res.IsError)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }

            // ✅ نمایش خطاها در ModelState
            foreach (var error in res.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetRequsetFoeEditQuery(id=1, _userId));
            if (model.Value == null) return NotFound();
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditSellerRequest model)
        {
            _userId = _authService.GetLoginUserId();
            if (!ModelState.IsValid) return View(model);
            var result = await _mediator.Send(new IsCityCorrectCommand(model.StateId, model.CityId));
            if (result.IsError == false)
            {
                ModelState.AddModelError("StateId", "لطفا استان و شهر فروشگاه خود را صحیح وارد کنید .");
                return View(model);
            }
            var res = await _mediator.Send(new EditRequestSellerCommand(_userId, model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<IActionResult> Detail(int id)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetSellerDetailForSellerQuery(id, _userId));
            if (model.Value == null) return NotFound();
            return View(model.Value);
        }
    }
}
