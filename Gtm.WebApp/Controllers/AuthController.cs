using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;
using Utility.Appliation;
using Utility.Contract;
using Gtm.Contract.UserContract.Command;
using Gtm.Application.UserApp.Command;

namespace Gtm.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;

        public AuthController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "/")
        {
            var register = new RegisterUser { ReturnUrl = returnUrl };
            return View(register);
        }

        [HttpPost]
        public async Task<IActionResult> Login(RegisterUser model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _mediator.Send(new RegisterUserCommand(model));


            if (result.IsError)
            {
                ModelState.AddModelError(nameof(model.Mobile), ValidationMessages.SystemErrorMessage);
                return View(model);
            }

            var loginModel = new LoginUser
            {
                ReturnUrl = model.ReturnUrl,
                Mobile = model.Mobile,
                Email = model.Email,
            };

            return View("LoginUser",loginModel) ;
        }


        [HttpPost]
      
        public async Task<IActionResult> LoginUser(LoginUser model)
        {
            model.Email = "mahsahossei@gmail.com";
            //if (!ModelState.IsValid)
            //    return View(model);

            var result = await _mediator.Send(new LoginUserCommand(model));

            if (result.IsError)
            {
                ModelState.AddModelError(nameof(model.Mobile), ValidationMessages.SystemErrorMessage);
                return View(model);
            }
            var authresult = new AuthModel { Avatar = result.Value.Avatar, FullName = result.Value.FullName, Mobile = result.Value.Mobile, UserId = result.Value.UserId };
            if (!_authService.Login(authresult))
            {
                ModelState.AddModelError("", "ورود با شکست مواجه شد.");
                return View(model);
            }

            TempData["SuccessLogin"] = true;
            return Redirect(model.ReturnUrl ?? "/");
        }

        public IActionResult AccessDenied() => View();
        public void logOut()
        {
            _authService.Logout();
        }
    }
} 