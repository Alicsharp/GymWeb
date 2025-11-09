using Gtm.Application.SeoApp.Command;
using Gtm.Application.SeoApp.Query;
using Gtm.Contract.SeoContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Seo
{
    [Area("Admin")]
    [Authorize]
    public class SeoController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;

        public SeoController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        [Route("/Admin/Seo/{id}/{where}")]
        public async Task<IActionResult> Index(int id, WhereSeo where)
        {
            var title = await _mediator.Send(new GetAdminSeoTitleQuery(where, id));
            ViewData["Title"] = title.Value;

            var model = await _mediator.Send(new GetSeoForEditCommand(id, where));
            return View(model.Value);
        }
        [HttpPost]
        [Route("/Admin/Seo/{id}/{where}")]
        public async Task<IActionResult> Index(int id, WhereSeo where, CreateSeo model)
        {
            var s = await _mediator.Send(new GetAdminSeoTitleQuery(where, id));
            ViewData["Title"] =s.Value;


            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new UbsertSeoCommand(model));


            if (res.IsError == false)
            {
                TempData["ok"] = true;

                return Redirect($"/Admin/Seo/{id}/{where}");
            }
            else
            {
                TempData["faild"] = true;
                return Redirect($"/Admin/Seo/{id}/{where}");
            }
        }
    }
}
