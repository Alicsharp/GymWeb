using Gtm.Application.PostServiceApp.PostApp.Command;
using Gtm.Application.PostServiceApp.PostApp.Query;
using Gtm.Application.PostServiceApp.PostSettingApp.Command;
using Gtm.Contract.PostContract.PostContract.Command;
using Gtm.Contract.PostContract.PostSettingContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Post
{
    [Area("Admin")]
    public class PostController : Controller
    {
        private readonly IMediator _mediator;

        public PostController(IMediator mediator)
        {
            _mediator = mediator;
        }


        public async Task<IActionResult> Index()
        {
            var res = await _mediator.Send(new GetAllPostsForAdminQuery());
            return View(res.Value);
        }
        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(CreatePost model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreatePostCommand(model));
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
            var res = await _mediator.Send(new GetForEditQuery(id));
            return View(res.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditPost model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditPostCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/Post/Edit/{id}");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<bool> Active(int id) { var res = await _mediator.Send(new PostActivationChangeCommand(id)); if (res.IsError) return false; return true; }
        public async Task<bool> Inside(int id) { var res = await _mediator.Send(new PostInsideCityChangeCommand(id)); if (res.IsError) return false; return true; }
        public async Task<bool> Outside(int id) { var res = await _mediator.Send(new PostOutSideCityChangeCommand(id)); if (res.IsError) return false; return true; }
        public async Task<IActionResult> Setting()
        {
            var res = await _mediator.Send(new GetForUbsertCommand());
            return View(res.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Setting(UbsertPostSetting model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new UbsertCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect("/Admin/Post/Setting/");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
    }
}
