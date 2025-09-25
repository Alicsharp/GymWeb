using Gtm.Application.EmailServiceApp.SensEmailApp.Command;
using Gtm.Application.EmailServiceApp.SensEmailApp.Query;
using Gtm.Contract.EmailContract.SensEmailContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Email
{
    [Area("Admin")]
    public class SendEmailController : Controller
    {
        private readonly IMediator _mediator;
        public SendEmailController(IMediator mediator)
        {
            _mediator = mediator;

        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetEmailSendsForAdminQuery());
            return View(result.Value);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _mediator.Send(new GetSendEmailDetailForAdminQuery(id));
            return View(result.Value);
        }
        public IActionResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(CreateSendEmail model)
        {

            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateSendEmailCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
    }
}
