using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp.Query
{
    public record GetCommentsForUiQuery(int TargetEntityId, CommentFor commentFor, int pageId ):IRequest<ErrorOr<CommentUiPaging>>;
    public class GetCommentsForUiQueryHandler : IRequestHandler<GetCommentsForUiQuery, ErrorOr<CommentUiPaging>>
    {
        private readonly ICommentRepo _commentRepository;
        private readonly IUserRepo _userRepository;
        private readonly ICommentValidator _validator;

        public GetCommentsForUiQueryHandler(ICommentRepo commentRepository,IUserRepo userRepository,ICommentValidator validator)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<CommentUiPaging>> Handle(GetCommentsForUiQuery request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetCommentsForUiAsync(
              request.TargetEntityId,
              request.commentFor,
              request.pageId);

            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // دریافت نظرات از دیتابیس
                var comments = await _commentRepository.GetAllByQueryAsync(
                    c => c.Status == CommentStatus.تایید_شده &&
                         c.TargetEntityId == request.TargetEntityId &&
                         c.CommentFor == request.commentFor,
                    cancellationToken);

                var model = new CommentUiPaging();

                // استفاده از ToList() برای تبدیل به لیست
                var commentsList = comments.ToList();
                model.GetData(commentsList.AsQueryable(), request.pageId, 3, 2);

                model.OwnerId = request.TargetEntityId;
                model.CommentFor = request.commentFor;
                model.Comments = new List<CommentUiQueryModel>();

                if (commentsList.Any())
                {
                    // دریافت نظرات والد
                    var parentComments = commentsList
                        .Where(r => r.ParentId == null)
                        .Skip(model.Skip)
                        .Take(model.Take)
                        .ToList();

                    model.Comments = parentComments
                        .Select(c => new CommentUiQueryModel
                        {
                            Avatar = FileDirectories.UserImageDirectory100 + "default.png",
                            Childs = commentsList
                                .Where(r => r.ParentId == c.Id)
                                .Select(r => new CommentUiQueryModel
                                {
                                    Avatar = FileDirectories.UserImageDirectory100 + "default.png",
                                    Childs = new List<CommentUiQueryModel>(),
                                    CreationDate = r.CreateDate.ToPersainDate(),
                                    FullName = r.FullName,
                                    Id = r.Id,
                                    Text = r.Text,
                                    AuthorUserId = r.AuthorUserId
                                })
                                .ToList(),
                            CreationDate = c.CreateDate.ToPersainDate(),
                            FullName = c.FullName,
                            Id = c.Id,
                            AuthorUserId = c.AuthorUserId,
                            Text = c.Text
                        })
                        .ToList();

                    // آپدیت آواتار کاربران
                    foreach (var comment in model.Comments)
                    {
                        if (comment.AuthorUserId > 0)
                        {
                            var user = await _userRepository.GetByIdAsync(comment.AuthorUserId);
                            comment.Avatar = FileDirectories.UserImageDirectory100 + (user?.Avatar ?? "default.png");
                        }

                        foreach (var child in comment.Childs)
                        {
                            if (child.AuthorUserId > 0)
                            {
                                var user = await _userRepository.GetByIdAsync(child.AuthorUserId);
                                child.Avatar = FileDirectories.UserImageDirectory100 + (user?.Avatar ?? "default.png");
                            }
                        }
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Comment.FetchError",
                    description: $"خطا در دریافت نظرات: {ex.Message}");
            }
        }
    }

}
