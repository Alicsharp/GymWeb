using Gtm.Application.PostServiceApp.PackageApp.Command;
using Gtm.Application.PostServiceApp.PackageApp.Query;
using Gtm.Application.PostServiceApp.UserPostApp.Query;
using Gtm.Contract.PostContract.UserPostContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Post
{
    [Area("Admin")]
    public class PackageController : Controller
    {
        private readonly IMediator _mediator;

        public PackageController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var post = await _mediator.Send(new GetAllPackageQuery());
            return View(post.Value);
        }
        public async Task<IActionResult> Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(CreatePackage model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreatePackgeCommand(model));

            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _mediator.Send(new GetForEditByIdPackageQuery(id));
            return View(res.Value);

        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditPackage model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditPackageCommand(model));

            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/Package/Edit/{id}");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async void Active(int id)
        {
            await _mediator.Send(new ActivationChangePackageCommand(id));

        }
    }
}
