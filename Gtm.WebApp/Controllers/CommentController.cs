using Gtm.Application.CommentApp.Command;
using Gtm.Application.CommentApp.Query;
using Gtm.Contract.CommentContract.Command;
using Gtm.Contract.CommentContract.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utility.Appliation.Auth;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Controllers
{ 
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        public CommentController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }
        

        [Route("/Comments/{ownerId}/{commentFor}")]
        public async Task<IActionResult> Comments(int ownerId, CommentFor commentFor, int pageId = 1)
        {
            var model = await _mediator.Send(new GetCommentsForUiQuery(ownerId, commentFor, pageId));
            if (model.Value == null) { return NotFound(); }
            var json = JsonConvert.SerializeObject(model.Value);
            return Json(json);
        }
        [HttpPost]
        public async Task<IActionResult> Create(int ownerId, CommentFor commentFor, long? parentId,string? email, string fullName, string text)
        {
             
            CreateCommentDto createComment = new CreateCommentDto
            {
                Email = email,
                CommentFor = commentFor,
                FullName =_authService.GetLoginUserFullName() ,
                OwnerId = ownerId,
                ParentId = parentId,
                Text = text,
                UserId = _authService.GetLoginUserId()
            };
            var res = await _mediator.Send(new CreateCommentCommand(createComment));
            if (res.IsError == false)
                TempData["SuccessCreateComment"] = true;
            else
                TempData["FaildCreateComment"] = true;
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet("/Comment/Childs/{parentId}")]
        public async Task<IActionResult> GetChilds(long parentId)
        {
            var result = await _mediator.Send(new GetChildCommentsQuery(parentId));
        
            return result.Match<IActionResult>(
                success => Json(success),
                errors => BadRequest(errors)
            );
        }

    }

    //[HttpGet]
    //public async Task<IActionResult> GetCommentReplies(long parentId)
    //{
    //    var comments = await _commentRepository.GetChildrenAsync(parentId);
    //    var model = comments.Select(c => new CommentUiQueryModel
    //    {
    //        Id = c.Id,
    //        Text = c.Text,
    //        UserId = c.AuthorUserId,
    //        FullName = c.FullName,
    //        CreationDate = c.CreateDate.ToPersainDate(),
    //        Avatar = FileDirectories.UserImageDirectory100 + "default.png" // در صورت نیاز با userRepo تغییر بده
    //    }).ToList();

    //    return PartialView("_CommentRepliesPartial", model);
    //}



}
       //[Route("/GetBestBlogs")]
        //public async Task<JsonResult> GetBestBlogs()
        //{
        //    //var model = await _mediator.Send(new GetBestBlogsForMagIndexQuery());
        //    var json = JsonConvert.SerializeObject(model.Value);
        //    return Json(json);

        //}
