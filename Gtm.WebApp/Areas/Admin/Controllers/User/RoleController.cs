using Gtm.Application.RoleApp.Command;
using Gtm.Application.RoleApp.Query;
using Gtm.Contract.RoleContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.User
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly IMediator _mediator;

        public RoleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _mediator.Send(new GetAllRolesQuery());
            return View(data.Value);
        }

        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(CreateRole model, List<UserPermission> permissions)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateRoleCommand(model, permissions));
            if (!res.IsError)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            foreach (var error in res.Errors)
            {
                ModelState.AddModelError(error.Code ?? "", error.Description);
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {

            var model = await _mediator.Send(new GetForEditQuery(id));
            var s = await _mediator.Send(new GetPermissionsForRoleQuery(id));
            ViewData["permissions"] = s.Value;
            return View(model.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRole model, List<UserPermission> permissions)
        {
            var permission = await _mediator.Send(new GetPermissionsForRoleQuery(id));
            ViewData["permissions"] = permission.Value;

            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditRoleCommand(model, permissions));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", ValidationMessages.ParentCategoryMessage);
            return View(model);
        }
    }
}
