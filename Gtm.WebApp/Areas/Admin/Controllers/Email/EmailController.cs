using Gtm.Application.EmailServiceApp.EmailUserApp.Command;
using Gtm.Application.EmailServiceApp.EmailUserApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Email
{
    [Area("Admin")]
    public class EmailController : Controller
    {
        private readonly IMediator _mediator;
        public EmailController(IMediator mediator)
        {
            _mediator = mediator;

        }

        public async Task<IActionResult> Index(int pageId = 1, int take = 10, string filter = "")
        {

            var result = await _mediator.Send(new GetEmailUsersForAdminQuery(pageId, take, filter));
            return View(result.Value);
        }
        public async Task<bool> Active(int id)
        {
            var res = await _mediator.Send(new ActivationChangeCommand(id));
            return res.Value;
        }

    }

}
