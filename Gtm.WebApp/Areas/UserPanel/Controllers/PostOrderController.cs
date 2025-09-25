using Gtm.Application.PostServiceApp.UserPostApp.Command;
using Gtm.Application.PostServiceApp.UserPostApp.Query;
using Gtm.Contract.PostContract.UserPostContract.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
    [Authorize]
    [Area("UserPanel")]
    public class PostOrderController : Controller
    {
        private int _userId;
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;

        public PostOrderController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Orders(int pageId = 0)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetPostOrdersForUserPanelQuery(pageId, _userId));
            return View(model.Value);
        }
        public async Task<IActionResult> Basket()
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetPostOrderNotPaymentForUserQuery(_userId));
            if (model.Value == null)
            {
                TempData["OrderNotExist"] = true;
                return Redirect("/Post");
            }
            return View(model.Value);
        }
        public async Task<IActionResult> Create(int id)
        {
            _userId = _authService.GetLoginUserId();
            var createpostOrder = await _mediator.Send(new GetCreatePostModelAsyncQuery(_userId, id));
           var s= await _mediator.Send(new CreatePostOrderCommand(createpostOrder.Value));
            return RedirectToAction("Basket");
        }
        public async Task<IActionResult> Payment()
        {

            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetPostOrderNotPaymentForUserQuery(_userId));
            if (model.Value == null)
            {
                TempData["OrderNotExist"] = true;
                return Redirect("/Post");
            }
            PaymentPostModel payment = new(_userId, 0, model.Value.Price);
            await _mediator.Send(new PaymentPostOrderCommand(payment));
            return RedirectToAction("Orders");
        }
        public async Task<IActionResult> UserPost()
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetUserPostModelForPanelQuery(_userId));
            return View(model.Value);
        }
    }
}
