 
using Gtm.Application.SiteServiceApp.SliderApp.Command;
using Gtm.Application.SiteServiceApp.SliderApp.Query;
using Gtm.Contract.SiteContract.SliderContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Site
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly IMediator _mediator;

        public SliderController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllSliderForAdminQuery());
            return View(result.Value);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlider model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateSliderCommand(model));
            if (res.IsError == false)
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
            var model = await _mediator.Send(new GetForEditQuery(id));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditSlider model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditSliderCommand(model));
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
            var res = await _mediator.Send(new SliderActivationChangeCommand(id));
            if (res.IsError) return false;
            return true;
        }
    }
}
