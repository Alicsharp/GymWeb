using Gtm.Application.CommentApp.Command;
using Gtm.Application.CommentApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Comment
{
    [Area("Admin")]
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;
        public CommentController(IMediator mediator)
        {
            _mediator = mediator;

        }

        [Route("/Admin/Comment/{id=1}/{type}/{status}")]
        public async Task<IActionResult> Index(int id, CommentFor type, CommentStatus status, long? parent = null, int pageId = 1, string filter = "", int take = 10)
        {
            var res = await _mediator.Send(new GetCommentForAdminQuery(pageId, take, filter, id, type, status, parent));
            return View(res.Value);
        }
        public async Task<IActionResult> UnSeen()
        {
            var res = await _mediator.Send(new GetAllUnSeenCommentsForAdminQuery());
            return View(res.Value);
        }
        public async Task<bool> Accept(int id)
        {
            var result = await _mediator.Send(new AcceptedCommentCommand(id));
           if(result.IsError) return false;
           return true; 
        }

        //public async Task<bool> Reject(long id, string why)
        //{
        //    var res = await _mediator.Send(new RejectCommentCommand(new RejectComment { Id = id, Why = why }));

        //    return res.Value;
        //}
    }
}
