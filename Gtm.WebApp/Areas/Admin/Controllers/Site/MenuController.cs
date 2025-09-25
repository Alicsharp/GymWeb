
using Gtm.Application.SiteServiceApp.MenuApp.Command;
using Gtm.Application.SiteServiceApp.MenuApp.Query;
using Gtm.Contract.SiteContract.MenuContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Site
{
    [Area("Admin")]
    public class MenuController : Controller
    {
        private readonly IMediator _mediator;

        public MenuController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index(int id = 0)
        {
            var result = await _mediator.Send(new GetForAdminQuery(id));

            return View(result.Value);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMenu model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _mediator.Send(new CreateMenuCommand(model));
            if (result.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect("/Admin/Menu/Index/0");
            }
            // اضافه کردن تمام خطاها
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                TempData["Error"] = error.Description; // فقط آخرین خطا
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<IActionResult> CreateSub(int id)
        {
            var res = await _mediator.Send(new GetForCreateQuery(id));
            return View(res.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSub(int id, CreateSubMenu model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateSubCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/Menu/Index/{id}");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {

            var model = await _mediator.Send(new GetMenuForEditQuery(id));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditMenu model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditMenuCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/Menu/Index/{model.ParentId}");
            }

            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var result = await _mediator.Send(new MenuActivationChangeCommand(id));
            if (result.IsError) return false;
            return true;
        }
    }
}
