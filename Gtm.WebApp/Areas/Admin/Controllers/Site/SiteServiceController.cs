
using Gtm.Application.SiteServiceApp.SiteServiceApp.Command;
using Gtm.Application.SiteServiceApp.SiteServiceApp.Query;
using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Site
{
    [Area("Admin")]
    public class SiteServiceController : Controller
    {
        private readonly IMediator _mediator;

        public SiteServiceController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllSiteSrviceForAdminQuery());
            return View(result.Value);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSiteService model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _mediator.Send(new CreateSiteServiceCommand(model)); ;
            if (result.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 2) return NotFound();

            var model = await _mediator.Send(new GetSiteServiceForEditQuery(id));
            return View(model.Value);

        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditSiteService model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _mediator.Send(new EditSiteServiceCommand(model)); ;
            if (result.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var result = await _mediator.Send(new ActivationChangeSiteServiceCommand(id));
            if (result.IsError) return false;
            return true;
        }

    }
}
