using ErrorOr;
using Gtm.Application.ArticleApp;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp.Query
{
    public record GetCommentForAdminQuery(int pageId, int take, string filter, int ownerId, CommentFor commentFor, CommentStatus status, long? parentId) : IRequest<ErrorOr<CommentAdminPaging>>;
    public class GetCommentForAdminQueryHandler : IRequestHandler<GetCommentForAdminQuery, ErrorOr<CommentAdminPaging>>
    {
        private readonly ICommentRepo _commentRepo;
        private readonly IArticleRepo _articleRepo;
        private readonly IUserRepo _userRepo;

        public GetCommentForAdminQueryHandler(ICommentRepo commentRepo, IArticleRepo articleRepo, IUserRepo userRepo)
        {
            _commentRepo = commentRepo;
            _articleRepo = articleRepo;
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<CommentAdminPaging>> Handle(GetCommentForAdminQuery request, CancellationToken cancellationToken)
        {
            var result = _commentRepo
                .QueryBy(c =>
                    c.CommentFor == request.commentFor &&
                    c.Status == request.status &&
                    c.TargetEntityId == request.ownerId &&
                    c.ParentId == request.parentId)
                .OrderByDescending(c => c.Id);

            if (!string.IsNullOrWhiteSpace(request.filter))
            {
                result = result
                    .Where(c =>
                        c.FullName.Contains(request.filter) ||
                        c.Email.Contains(request.filter) ||
                        c.Text.Contains(request.filter))
                    .OrderByDescending(c => c.Id);
            }

            CommentAdminPaging model = new();
            model.GetData(result, request.pageId, request.take, 5);
            model.Filter = request.filter;
            model.CommentFor = request.commentFor;
            model.CommentStatus = request.status;
            model.OwnerId = request.ownerId;
            model.ParentId = request.parentId;
            model.PageTitle = $"لیست نظرات - {request.status.ToString().Replace("_", " ")} - {request.commentFor.ToString().Replace("_", " ")}";

            var pagedComments = result.Skip(model.Skip).Take(model.Take).ToList();

            model.Comments = pagedComments.Select(c => new CommentAdminQueryModel
            {
                CommentFor = c.CommentFor,
                Email = c.Email,
                FullName = c.FullName,
                HaveChild = false,
                Id = c.Id,
                OwnerId = c.TargetEntityId,
                ParentId = c.ParentId,
                Status = c.Status,
                Text = c.Text,
                UserId = c.AuthorUserId,
                WhyRejected = c.WhyRejected,
                CommentTitle = "",
                UserName = ""
            }).ToList();

            // تنظیم عنوان صفحه بر اساس موجودیت مالک
            if (model.OwnerId > 0)
            {
                switch (model.CommentFor)
                {
                    case CommentFor.مقاله:
                        var blog = await _articleRepo.GetByIdAsync(model.OwnerId);
                        model.PageTitle += $"  {blog.Title}";
                        break;

                    case CommentFor.محصول:
                        // در صورت نیاز پیاده‌سازی شود
                        break;

                    default:
                        break;
                }
            }

            // اگر پاسخ به کامنت خاصی است
            if (model.ParentId is > 0)
            {
                var parentComment = await _commentRepo.GetByIdAsync(model.ParentId.Value);
                model.PageTitle += $"- پاسخ برای {parentComment.FullName}";
            }

            // پردازش اطلاعات اضافی برای هر کامنت
            foreach (var comment in model.Comments)
            {
                comment.HaveChild = await _commentRepo.ExistsAsync(c => c.ParentId == comment.Id);

                if (comment.UserId > 0)
                {
                    var user = await _userRepo.GetByIdAsync(comment.UserId);
                    comment.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                }

                switch (comment.CommentFor)
                {
                    case CommentFor.مقاله:
                        var blog = await _articleRepo.GetByIdAsync(comment.OwnerId);
                        comment.CommentTitle = $"نظر برای مقاله {blog.Title}";
                        break;

                    case CommentFor.محصول:
                        // در صورت نیاز پیاده‌سازی شود
                        break;

                    default:
                        break;
                }
            }

            return model;
        }
    }

}

