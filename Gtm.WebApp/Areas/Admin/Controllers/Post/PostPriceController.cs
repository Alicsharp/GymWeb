using Gtm.Application.PostServiceApp.PostApp.Query;
using Gtm.Application.PostServiceApp.PostPriceApp.Command;
using Gtm.Application.PostServiceApp.PostPriceApp.Query;
using Gtm.Contract.PostContract.PostPriceContract.Command;
using Gtm.Domain.PostDomain.PostPriceAgg;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Post
{
    [Area("Admin")]
    public class PostPriceController : Controller
    {
        private readonly IMediator _mediator;

        public PostPriceController(IMediator mediator)
        {
            _mediator = mediator;
        }


        public async Task<IActionResult> Index(int id)
        {
            var res = await _mediator.Send(new GetPostDetailsQuery(id));
            return View(res.Value);
        }
        public IActionResult Create(int id) => View(new CreatePostPrice { PostId = id });
        [HttpPost]
        public async Task<IActionResult> Create(int id, CreatePostPrice model)
        {
            if (id != model.PostId) return NotFound();
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreatePostPriceCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/PostPrice/Index/{id}");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _mediator.Send(new GetPostPriceForEditQuery(id));
            return View(res.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditPostPrice model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditPostPriceCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/PostPrice/Edit/{id}");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
    }
}
