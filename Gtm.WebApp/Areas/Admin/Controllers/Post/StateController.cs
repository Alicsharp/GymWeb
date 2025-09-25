using Gtm.Application.PostServiceApp.StateApp.Command;
using Gtm.Application.PostServiceApp.StateApp.Query;
using Gtm.Contract.PostContract.StateContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Post
{
    [Area("Admin")]
    public class StateController : Controller
    {
        private readonly IMediator _mediator;

        public StateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var res = await _mediator.Send(new GetStatesForAdminQuery());

            return View(res.Value);
        }
        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(CreateStateModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateStateCommand(model));
            if (!res.IsError)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(res.FirstError.Code, res.FirstError.Description);
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _mediator.Send(new GetStateForEditQuery(id));
            return View(res.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditStateModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditStateCommand(model));

            if (!res.IsError)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<IActionResult> ChangeClose(int id, List<int>? stateCloses)
        {
            if (stateCloses.Count() < 1)
            {
                TempData["ChooseState"] = true;
                return Redirect($"/Admin/City/Index/{id}");
            }
            var res = await _mediator.Send(new ChangeStateCloseCommand(id, stateCloses));
            if (res.IsError==false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/City/Index/{id}");
            }
            else
            {
                TempData["faild"] = true;
                return Redirect($"/Admin/City/Index/{id}");
            }
        }
    }
}
