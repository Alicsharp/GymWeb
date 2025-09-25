using Gtm.Application.EmailServiceApp.MessageUserApp.Command;
using Gtm.Application.EmailServiceApp.MessageUserApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Email
{
    [Area("Admin")]
    public class MessageController : Controller
    {
        private readonly IMediator _mediator;
        public MessageController(IMediator mediator)
        {
            _mediator = mediator;

        }

        public async Task<IActionResult> Index(int pageId = 1, int take = 10, string filter = "", MessageStatus status = MessageStatus.همه)
        {
            var res = await _mediator.Send(new GetMessagesForAdminQuery(pageId, take, filter, status));
            return View(res.Value);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var res = await _mediator.Send(new GetMessageDetailForAdminQuery(id));
            return View(res.Value);

        }
        public async Task<bool> ChangeStatus(int id, string answer, MessageStatus status)
        {
            switch (status)
            {
                case MessageStatus.پاسخ_داده_شد_sms:
                    if (!string.IsNullOrEmpty(answer))
                    {
                        // send sms
                    }
                    var res = await _mediator.Send(new AnsweredBySMSCommand(id, answer));
                    return res.Value;
                case MessageStatus.پاسخ_داده_شد_email:
                    if (!string.IsNullOrEmpty(answer))
                    {
                        // send email
                    }
                    var result = await _mediator.Send(new AnsweredByEmailCommand(id, answer));
                    return result.Value;
                default:
                    var rest = await _mediator.Send(new AnswerByCallCommand(id));
                    return rest.Value;
            }
        }
    }
}
