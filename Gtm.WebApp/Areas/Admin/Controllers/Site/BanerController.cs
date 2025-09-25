
using Gtm.Application.SiteServiceApp.BannerApp.Command;
using Gtm.Application.SiteServiceApp.BannerApp.Query;
using Gtm.Contract.SiteContract.BanarContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Site
{
    [Area("Admin")]
    public class BanerController : Controller
    {
        private readonly IMediator _mediator;

        public BanerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllForAdminQuery());
            return View(result.Value);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBaner model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateBannerCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            TempData["fail"]=true;
            ModelState.AddModelError("", "");
            return View(model);

        }
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 2) return NotFound();
            var model = await _mediator.Send(new GetForEditQuery(id));
            return View(model.Value);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditBaner model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditBannerCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var result = await _mediator.Send(new ActivationChangeCommand(id));
            if(result.IsError) return false;
            return true;
        }
    }
}
