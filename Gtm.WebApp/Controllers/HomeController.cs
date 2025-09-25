using Gtm.Application.EmailServiceApp.EmailUserApp.Command;
using Gtm.Application.EmailServiceApp.MessageUserApp.Command;
using Gtm.Application.SiteServiceApp.SitePageApp.Query;
using Gtm.Application.SiteServiceApp.SiteServiceApp.Query;
using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using Gtm.Contract.EmailContract.EmailUserContract.Command;
using Gtm.Contract.EmailContract.MessageUserContract.Command;
using Gtm.WebApp.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Utility.Appliation;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        public HomeController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Error()
        {
            return View();
        }

        //ÈÑÑÓ? ÔæÏ
        [HttpPost]
        public async Task<string> AddEmailUser(string email)
        {
            var userId = _authService.GetLoginUserId();
            CreateEmailUser model = new()
            {
                Email = email,
                UserId = userId
            };
            var res = await _mediator.Send(new CreateEmailUserCommand(model));
            if (res.IsError == false) return "";
            return "";
        }
        [Route("/Contact")]
        public async Task<IActionResult> Contact()
        {
            var model = await _mediator.Send(new GetContactUsModelForUiQuery());
            return View(model.Value);
        }
        [Route("/Page/{slug}")]
        public async Task<IActionResult> Page(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();
            var model = await _mediator.Send(new GetSitePageQueryModelQuery(slug));
            if (model.Value == null) return NotFound();
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(string fullName, string email, string subject, string message)
        {
            var userId = _authService.GetLoginUserId();
            if (string.IsNullOrEmpty(fullName) ||
                (string.IsNullOrEmpty(email) || (email.IsEmail() == false && email.IsMobile() == false))
                || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                TempData["FaildMessage"] = true;
                return RedirectToAction("Contact");
            }
            var res = await _mediator.Send(new CreateMessageCommand(new CreateMessageUser
            {
                Email = email.IsEmail() ? email : "",
                FullName = fullName,
                Message = message,
                PhoneNumber = email.IsMobile() ? email : "",
                Subject = subject,
                UserId = userId
            }));
            if (res.IsError == false)
            {
                TempData["SuccessMessage"] = true;
                return RedirectToAction("Contact");
            }
            else
            {
                TempData["FaildMessage"] = true;
                return RedirectToAction("Contact");
            }
        }
        [Route("/About")]
        public async Task<IActionResult> About()
        {
            var model = await _mediator.Send(new GetAboutUsModelForUiQuery());
            return View(model.Value);
        }
    }
}