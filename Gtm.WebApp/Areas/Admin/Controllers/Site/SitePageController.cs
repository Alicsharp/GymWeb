
using ErrorOr;
using Gtm.Application.SiteServiceApp.SitePageApp.Command;
using Gtm.Application.SiteServiceApp.SitePageApp.Query;
using Gtm.Contract.SiteContract.SitePageContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Site
{
    [Area("Admin")]
    public class SitePageController : Controller
    {
        private readonly IMediator _mediator;

        public SitePageController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllSitePageForAdminQuery());
            return View(result.Value);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSitePage model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateSitePageCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            TempData["Error"] = res.FirstError.Description;
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
        public async Task<IActionResult> Edit(int id, EditSitePage model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditSitePageCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/SitePage");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var res = await _mediator.Send(new ActivationChangeCommand(id));
            if (res.IsError) return false;
            return true;
        }
    }
}
