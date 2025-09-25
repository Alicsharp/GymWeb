using Gtm.Application.PostServiceApp.CityApp.Command;
using Gtm.Application.PostServiceApp.StateApp.Query;
using Gtm.Contract.PostContract.CityContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Post
{
    [Area("Admin")]
    public class CityController : Controller
    {
        private readonly IMediator _mediator;

        public CityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int id)
        {
            var res = await _mediator.Send(new GetStateDetailQuery(id));

            return View(res.Value);
        }
        public async Task<IActionResult> Create(int id)
        {
            var title = await _mediator.Send(new GetStateTitleQuery(id));
            ViewData["Title"] = "افزودن شهر به " + title.Value;

            return View(new CreateCityModel { StateId = id });
        }
        [HttpPost]
        public async Task<IActionResult> Create(int id, CreateCityModel model)
        {
            if (id != model.StateId) return NotFound();
            ViewData["Title"] = "افزودن شهر به " + await _mediator.Send(new GetStateTitleQuery(id));
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateCityCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/City/Index/{model.StateId}");

            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _mediator.Send(new GetStateForEditQuery(id));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditCityModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditCityCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<bool> Status(int id, CityStatus status)
        { 
            var res = await _mediator.Send(new ChangeStatusCommand(id, status)); 
            if(res.IsError)
            {
                return false;
            }
            return true;
        }
    }
}
